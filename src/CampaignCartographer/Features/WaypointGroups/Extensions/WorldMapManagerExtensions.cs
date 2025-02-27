using ApacheTech.Common.Extensions.Harmony;

namespace ApacheTech.VintageMods.CampaignCartographer.Features.WaypointGroups.Extensions;

/// <summary>
///     Provides extension methods for <see cref="WorldMapManager"/>.
/// </summary>
public static class WorldMapManagerExtensions
{
    /// <summary>
    ///     Builds a world map dialogue instance for the specified map manager.
    /// </summary>
    /// <param name="mapManager">The world map manager.</param>
    public static void BuildWorldMapDialogue(this WorldMapManager mapManager)
    {
        if (mapManager.worldMapDlg is null) return;
        var onViewChangedClient = new OnViewChangedDelegate((nowVisible, nowHidden)
            => mapManager.CallMethod("onViewChangedClient", nowVisible, nowHidden));
        var getTabsOrdered = mapManager.CallMethod<List<string>>("getTabsOrdered");
        mapManager.worldMapDlg = new GuiDialogWorldMap(onViewChangedClient, ApiEx.Client, getTabsOrdered);
    }

    /// <summary>
    ///     Unregisters a map layer from the world map manager using the specified key.
    /// </summary>
    /// <param name="mapManager">The world map manager.</param>
    /// <param name="key">The key identifying the map layer to remove.</param>
    public static void UnregisterMapLayer(this WorldMapManager mapManager, string key)
    {
        mapManager.MapLayerRegistry.RemoveIfPresent(key);
        mapManager.LayerGroupPositions.RemoveIfPresent(key);
        mapManager.MapLayers.RemoveAll(p => p.LayerGroupCode == key);
    }
}