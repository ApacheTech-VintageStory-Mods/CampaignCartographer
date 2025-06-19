using ApacheTech.VintageMods.CampaignCartographer.Features.ModMenu.Extensions;
using ApacheTech.VintageMods.CampaignCartographer.Features.WaypointGroups.Abstractions;
using ApacheTech.VintageMods.CampaignCartographer.Features.WaypointGroups.Dialogue;
using ApacheTech.VintageMods.CampaignCartographer.Features.WaypointGroups.MapLayers;
using ApacheTech.VintageMods.CampaignCartographer.Features.WaypointGroups.Models;
using Gantry.Core.Extensions.DotNet;
using Gantry.Core.Hosting.Registration;
using Gantry.Services.FileSystem.Hosting;

namespace ApacheTech.VintageMods.CampaignCartographer.Features.WaypointGroups.Systems;

/// <summary>
///     Manages waypoint groups within the world map, providing utilities for retrieving, 
///     modifying, and interacting with waypoint groups and their corresponding map layers.
/// </summary>
[HarmonyClientSidePatch]
public class WaypointGroups : ClientModSystem, IClientServiceRegistrar
{
    /// <summary>
    ///     The world map manager instance used to interact with the map layers.
    /// </summary>
    private static WorldMapManager MapManager { get; set; } = null!;

    /// <summary>
    ///     The settings instance containing all waypoint groups.
    /// </summary>
    private static WaypointGroupsSettings Settings { get; set; } = null!;

    /// <inheritdoc />
    public void ConfigureClientModServices(IServiceCollection services, ICoreClientAPI capi)
    {
        services.AddFeatureWorldSettings<WaypointGroupsSettings>();
        services.TryAddTransient<WaypointGroupsDialogue>();
        services.AddSingleton<IMapLayerGeneration, MapLayerGeneration>();
    }

    /// <inheritdoc />
    public override void StartClientSide(ICoreClientAPI api)
    {
        base.StartClientSide(api);
        api.AddModMenuDialogue<WaypointGroupsDialogue>("WaypointGroups");
        Settings = IOC.Services.GetRequiredService<WaypointGroupsSettings>();
        MapManager = api.ModLoader.GetModSystem<WorldMapManager>();
    }

    /// <summary>
    ///     Retrieves a dictionary of waypoint group identifiers and their corresponding titles.
    /// </summary>
    /// <returns>A dictionary mapping waypoint group IDs to their titles.</returns>
    public static Dictionary<string, string> GetWaypointGroupListItems()
    {
        var items = new Dictionary<string, string>()
        {
            [string.Empty] = T("DefaultGroupName")
        };

        foreach (var group in Settings.Groups)
        {
            items[group.Id.ToString()] = group.Title;
        }

        return items;
    }

    /// <summary>
    ///     Clears all waypoint map components for every waypoint group layer.
    /// </summary>
    public static void ClearAllGroupMapComponents()
        => MapManager.MapLayers.OfType<WaypointGroupMapLayer>().InvokeForAll(p => p.ClearComponents());

    /// <summary>
    ///     Retrieves the waypoint group associated with the specified waypoint.
    /// </summary>
    /// <param name="waypoint">The waypoint to look up.</param>
    /// <returns>The waypoint group that contains the waypoint, or <c>null</c> if none is found.</returns>
    public static WaypointGroup GetWaypointGroup(Waypoint waypoint)
         => Settings.Groups.FirstOrDefault(p => p.Waypoints.Select(p => p.ToString()).Contains(waypoint.Guid))!;

    /// <summary>
    ///     Retrieves the identifier of the waypoint group associated with the specified waypoint.
    /// </summary>
    /// <param name="waypoint">The waypoint to look up.</param>
    /// <returns>
    ///     The waypoint group ID as a string, or <c>null</c> if the waypoint does not belong to a group.
    /// </returns>
    public static string GetWaypointGroupId(Waypoint waypoint)
         => Settings.Groups.FirstOrDefault(p => p.Waypoints.Select(p => p.ToString()).Contains(waypoint.Guid))?.Id.ToString() ?? string.Empty;

    /// <summary>
    ///     Retrieves the map layer corresponding to the waypoint group that contains the specified waypoint.
    /// </summary>
    /// <param name="waypoint">The waypoint to look up.</param>
    /// <returns>
    ///     The <see cref="WaypointGroupMapLayer"/> instance associated with the waypoint group, 
    ///     or <c>null</c> if no such group exists.
    /// </returns>
    public static WaypointGroupMapLayer? GetWaypointGroupMapLayer(Waypoint waypoint)
    {
        var group = GetWaypointGroup(waypoint);
        if (group is null) return null;
        return MapManager.MapLayers.FirstOrDefault(p => p.LayerGroupCode == group.Id.ToString())?.To<WaypointGroupMapLayer>();
    }

    /// <summary>
    ///     Retrieves a translated string for the given path within the "WaypointGroups" feature.
    /// </summary>
    /// <param name="path">The translation key.</param>
    /// <param name="args">Optional arguments to format the translated string.</param>
    /// <returns>The translated string.</returns>
    public static string T(string path, params object[] args)
        => LangEx.FeatureString("WaypointGroups", path, args);
}