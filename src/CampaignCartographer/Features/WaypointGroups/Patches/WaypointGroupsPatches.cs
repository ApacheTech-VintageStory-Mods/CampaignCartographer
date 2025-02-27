using ApacheTech.VintageMods.CampaignCartographer.Features.WaypointGroups.MapLayer.Commands;
using Gantry.Services.FileSystem.Configuration.Consumers;

namespace ApacheTech.VintageMods.CampaignCartographer.Features.WaypointGroups.Patches;

/// <summary>
///     Applies Harmony patches related to waypoint groups.
/// </summary>
[HarmonyClientSidePatch]
public class WaypointGroupsPatches : WorldSettingsConsumer<WaypointGroupsSettings>
{
    /// <summary>
    ///     Overrides the language key resolution for map layer names to use waypoint group titles.
    /// </summary>
    /// <param name="key">The language key being requested.</param>
    /// <param name="__result">The resolved string result.</param>
    /// <returns>
    ///     <c>false</c> if the key corresponds to a known waypoint group, preventing further processing; 
    ///     otherwise, <c>true</c>.
    /// </returns>
    [HarmonyPrefix]
    [HarmonyPriority(Priority.Last)]
    [HarmonyPatch(typeof(Lang), nameof(Lang.Get))]
    public static bool Harmony_Lang_Get_Postfix(string key, ref string __result)
    {
        if (!key.StartsWith("maplayer-")) return true;
        if (!Guid.TryParse(key.Replace("maplayer-", ""), out var id)) return true;
        var group = Settings.Groups.FirstOrDefault(p => p.Id == id);
        if (group is null) return true;
        __result = group.Title;
        return false;
    }

    /// <summary>
    ///     Ensures waypoint groups are added to the world map manager upon level finalisation.
    /// </summary>
    [HarmonyPostfix]
    [HarmonyPatch(typeof(WorldMapManager), "OnLvlFinalize")]
    public static void Harmony_WorldMapManager_OnLvlFinalize_Postfix()
    {
        foreach (var group in Settings.Groups)
        {
            IOC.Brighter.Send(new AddWaypointGroupLayerCommand() { Group = group });
        }
    }
}