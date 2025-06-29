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

    /// <summary>
    ///     Scans all player waypoints and removes duplicate entries with matching GUIDs per player.
    ///     If duplicates are found, data from duplicates is merged into the primary record, favouring
    ///     the first non-empty values encountered.
    /// </summary>
    /// <param name="waypoints">The waypoint list to deduplicate and merge in-place.</param>
    public static void DedupeWaypoints(this WaypointMapLayer waypointMapLayer)
    {
        var waypoints = waypointMapLayer.Waypoints;
        var grouped = waypoints
            .Where(wp => wp.Guid is not null && wp.OwningPlayerUid is not null)
            .GroupBy(wp => (wp.OwningPlayerUid, wp.Guid))
            .Where(g => g.Count() > 1);

        foreach (var group in grouped)
        {
            var merged = group.First();
            foreach (var duplicate in group.Skip(1))
            {
                if (string.IsNullOrWhiteSpace(merged.Title) && !string.IsNullOrWhiteSpace(duplicate.Title))
                    merged.Title = duplicate.Title;

                if (string.IsNullOrWhiteSpace(merged.Text) && !string.IsNullOrWhiteSpace(duplicate.Text))
                    merged.Text = duplicate.Text;

                if (merged.Position is null || merged.Position.X == 0 && merged.Position.Y == 0 && merged.Position.Z == 0)
                    merged.Position = duplicate.Position;

                if (merged.Color == 0) merged.Color = duplicate.Color;
                if (string.IsNullOrWhiteSpace(merged.Icon)) merged.Icon = duplicate.Icon;

                merged.ShowInWorld |= duplicate.ShowInWorld;
                merged.Pinned |= duplicate.Pinned;
                merged.Temporary &= duplicate.Temporary;
            }

            foreach (var dup in group.Skip(1))
                waypoints.Remove(dup);
        }
    }
}