#nullable enable
using ApacheTech.Common.Extensions.Harmony;
using Gantry.Services.FileSystem.Configuration;
using Gantry.Services.FileSystem.Configuration.Consumers;

namespace ApacheTech.VintageMods.CampaignCartographer.Features.WaypointGroups.Patches;

/// <summary>
///     Applies Harmony patches to manage waypoint map layers and their interactions with waypoint groups.
/// </summary>
[HarmonyClientSidePatch]
public class WaypointMapLayerPatches : WorldSettingsConsumer<WaypointGroupsSettings>
{
    private static readonly object _localLock = new();

    /// <summary>
    ///     Adjusts waypoint map components after rebuilding to ensure waypoints in groups 
    ///     are assigned to their respective group layers.
    /// </summary>
    /// <param name="___mapSink">The world map manager instance.</param>
    /// <param name="___wayPointComponents">The list of waypoint components in the map layer.</param>
    [HarmonyPostfix]
    [HarmonyPatch(typeof(WaypointMapLayer), "RebuildMapComponents")]
    public static void Harmony_WaypointMapLayer_RebuildMapComponents_Postfix(
        IWorldMapManager ___mapSink,
        ref List<MapComponent> ___wayPointComponents
    )
    {
        lock (_localLock)
        {
            if (!___mapSink.IsOpened) return;
            var inGroup = new List<MapComponent>();
            Systems.WaypointGroups.ClearAllGroupMapComponents();
            foreach (var mapComponent in ___wayPointComponents)
            {
                var waypoint = mapComponent.GetField<Waypoint>("waypoint");
                var group = Systems.WaypointGroups.GetWaypointGroupMapLayer(waypoint);
                if (group is null) continue;
                inGroup.Add(mapComponent);
                group.AddComponent(mapComponent as WaypointMapComponent);
            }

            ___wayPointComponents = [.. ___wayPointComponents.Except(inGroup)];
        }
    }

    /// <summary>
    ///     Ensures waypoint components are removed from their respective groups when the map is closed.
    /// </summary>
    /// <param name="___tmpWayPointComponents">The temporary list of waypoint components.</param>
    [HarmonyPrefix]
    [HarmonyPatch(typeof(WaypointMapLayer), "OnMapClosedClient")]
    public static void Harmony_WaypointMapLayer_OnMapClosedClient_Prefix(ref List<MapComponent> ___tmpWayPointComponents)
    {
        var inGroup = new List<MapComponent>();
        foreach (var mapComponent in ___tmpWayPointComponents)
        {
            var waypoint = mapComponent.GetField<Waypoint>("waypoint");
            var group = Systems.WaypointGroups.GetWaypointGroupMapLayer(waypoint);
            if (group is null) continue;
            inGroup.Add(mapComponent);
            group.RemoveComponent(mapComponent as WaypointMapComponent);
        }
    }

    /// <summary>
    ///     Removes orphaned waypoints from groups when data is received from the server.
    /// </summary>
    /// <param name="___ownWaypoints">The collection of waypoints owned by the player.</param>
    [HarmonyPostfix]
    [HarmonyPriority(Priority.Last)]
    [HarmonyPatch(typeof(WaypointMapLayer), nameof(WaypointMapLayer.OnDataFromServer))]
    public static void Harmony_WaypointMapLayer_OnDataFromServer_Postfix_Last(IEnumerable<Waypoint> ___ownWaypoints)
    {
        var saveChanges = false;
        try
        {
            var waypointGuidSet = ___ownWaypoints
                .Select(p =>
                {
                    if (!Guid.TryParse(p.Guid, out var waypointGuid)) return null;
                    return (Guid?)waypointGuid;
                })
                .Where(p => p.HasValue)
                .Select(p => p!.Value)
                .ToHashSet();

            foreach (var group in Settings.Groups)
            {
                var orphans = group.Waypoints.Where(p => !waypointGuidSet.Contains(p)).ToArray();
                if (orphans.Length == 0) continue;
                group.Waypoints = [.. group.Waypoints.Except(orphans)];
                saveChanges = true;
            }
        }
        catch (Exception ex)
        {
            G.Log.Error(ex);
        }
        finally
        {
            if (saveChanges) ModSettings.World.Save(Settings);
        }
    }
}