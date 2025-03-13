namespace ApacheTech.VintageMods.CampaignCartographer.Features.WaypointGroups.Extensions;

/// <summary>
///     Provides extension methods for <see cref="WorldMapManager"/>.
/// </summary>
public static class WorldMapManagerExtensions
{
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