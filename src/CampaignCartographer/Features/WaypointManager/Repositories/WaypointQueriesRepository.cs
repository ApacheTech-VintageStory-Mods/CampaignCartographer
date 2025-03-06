using ApacheTech.VintageMods.CampaignCartographer.Features.WaypointManager.Dialogues.WaypointSelection;
using ApacheTech.VintageMods.CampaignCartographer.Features.WaypointManager.Extensions;
using ApacheTech.VintageMods.CampaignCartographer.Features.WaypointManager.Models;
using ApacheTech.VintageMods.CampaignCartographer.Features.WaypointManager.WaypointTemplates;
using Gantry.Core.Extensions.DotNet;
using Vintagestory.API.MathTools;

namespace ApacheTech.VintageMods.CampaignCartographer.Features.WaypointManager.Repositories;

/// <summary>
///     Provides functionality for querying and sorting waypoints based on various criteria.
/// </summary>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public class WaypointQueriesRepository
{
    private readonly WorldMapManager _mapManager;
    private WaypointMapLayer _mapLayer;

    /// <summary>
    ///     Initialises a new instance of the <see cref="WaypointQueriesRepository"/> class.
    /// </summary>
    /// <param name="mapManager">The manager responsible for handling world map data.</param>
    public WaypointQueriesRepository(WorldMapManager mapManager)
    {
        _mapManager = mapManager;
    }

    /// <summary>
    ///     Converts and sorts a collection of waypoint templates into waypoints based on the specified order.
    /// </summary>
    /// <param name="waypoints">The collection of waypoint templates to sort.</param>
    /// <param name="sortOrder">The type of sorting to apply.</param>
    /// <returns>
    ///     An enumerable of key-value pairs, where the key is the waypoint's index, 
    ///     and the value is the corresponding <see cref="Waypoint"/>.
    /// </returns>
    public static IEnumerable<KeyValuePair<int, Waypoint>> SortWaypoints(
        IEnumerable<PositionedWaypointTemplate> waypoints,
        WaypointSortType sortOrder)
    {
        var list = waypoints
            .Select(x => x.ToWaypoint())
            .ToList()
            .ToSortedDictionary();
        return Sort(sortOrder, list);
    }

    /// <summary>
    ///     Retrieves and sorts waypoints, projecting each waypoint into a custom type.
    /// </summary>
    /// <typeparam name="T">The type to map each waypoint to.</typeparam>
    /// <param name="sortOrder">The type of sorting to apply.</param>
    /// <param name="mapper">A function that maps a <see cref="Waypoint"/> to the desired type <typeparamref name="T"/>.</param>
    /// <returns>
    ///     An enumerable of key-value pairs, where the key is the waypoint's index, 
    ///     and the value is the mapped type <typeparamref name="T"/>.
    /// </returns>
    public IEnumerable<KeyValuePair<int, T>> GetSortedWaypoints<T>(
        WaypointSortType sortOrder,
        System.Func<Waypoint, T> mapper)
    {
        return GetSortedWaypoints(sortOrder)
            .Select(x => new KeyValuePair<int, T>(x.Key, mapper(x.Value)));
    }

    /// <summary>
    ///     Retrieves and sorts all waypoints based on the specified sorting order.
    /// </summary>
    /// <param name="sortOrder">The type of sorting to apply.</param>
    /// <returns>
    ///     An enumerable of key-value pairs, where the key is the waypoint's index, 
    ///     and the value is the corresponding <see cref="Waypoint"/>.
    /// </returns>
    public IEnumerable<KeyValuePair<int, Waypoint>> GetSortedWaypoints(WaypointSortType sortOrder)
    {
        _mapLayer ??= _mapManager.WaypointMapLayer();
        var waypoints = _mapLayer.ownWaypoints.ToSortedDictionary();
        return Sort(sortOrder, waypoints);
    }

    /// <summary>
    ///     Sorts a collection of waypoints based on the specified sorting order.
    /// </summary>
    /// <param name="sortOrder">The type of sorting to apply.</param>
    /// <param name="waypoints">The collection of waypoints to sort, represented as a sorted dictionary.</param>
    /// <returns>
    ///     An enumerable of key-value pairs, where the key is the waypoint's index, 
    ///     and the value is the corresponding <see cref="PositionedWaypointTemplate"/>, ordered as specified.
    /// </returns>
    public static IEnumerable<WaypointSelectionCellEntry> SortCells(
        WaypointSortType sortOrder,
        IEnumerable<WaypointSelectionCellEntry> waypoints)
    {
        var playerPos = ApiEx.Client.World.Player.Entity.Pos.AsBlockPos;
        return sortOrder switch
        {
            WaypointSortType.IndexAscending => waypoints.OrderBy(p => p.Index),
            WaypointSortType.IndexDescending => waypoints.OrderByDescending(p => p.Index),
            WaypointSortType.ColourAscending => waypoints.OrderBy(p => ColorUtil.Int2Hex(p.Model.Colour.ToInt())),
            WaypointSortType.ColourDescending => waypoints.OrderByDescending(p => ColorUtil.Int2Hex(p.Model.Colour.ToInt())),
            WaypointSortType.NameAscending => waypoints.OrderBy(p => p.Model.Title),
            WaypointSortType.NameDescending => waypoints.OrderByDescending(p => p.Model.Title),
            WaypointSortType.DistanceAscending => waypoints.OrderBy(p =>
                p.Model.Position.AsBlockPos.HorizontalManhattenDistance(playerPos)),
            WaypointSortType.DistanceDescending => waypoints.OrderByDescending(p =>
                p.Model.Position.AsBlockPos.HorizontalManhattenDistance(playerPos)),
            _ => waypoints
        };
    }

    /// <summary>
    ///     Sorts a collection of waypoints based on the specified sorting order.
    /// </summary>
    /// <param name="sortOrder">The type of sorting to apply.</param>
    /// <param name="waypoints">The collection of waypoints to sort, represented as a sorted dictionary.</param>
    /// <returns>
    ///     An enumerable of key-value pairs, where the key is the waypoint's index, 
    ///     and the value is the corresponding <see cref="Waypoint"/>, ordered as specified.
    /// </returns>
    public static IEnumerable<KeyValuePair<int, Waypoint>> Sort(
        WaypointSortType sortOrder,
        SortedDictionary<int, Waypoint> waypoints)
    {
        var playerPos = ApiEx.Client.World.Player.Entity.Pos.AsBlockPos;
        return sortOrder switch
        {
            WaypointSortType.IndexAscending => waypoints,
            WaypointSortType.IndexDescending => waypoints.Reverse(),
            WaypointSortType.ColourAscending => waypoints.OrderBy(p => ColorUtil.Int2Hex(p.Value.Color)),
            WaypointSortType.ColourDescending => waypoints.OrderByDescending(p => ColorUtil.Int2Hex(p.Value.Color)),
            WaypointSortType.NameAscending => waypoints.OrderBy(p => p.Value.Title),
            WaypointSortType.NameDescending => waypoints.OrderByDescending(p => p.Value.Title),
            WaypointSortType.DistanceAscending => waypoints.OrderBy(p =>
                p.Value.Position.AsBlockPos.HorizontalManhattenDistance(playerPos)),
            WaypointSortType.DistanceDescending => waypoints.OrderByDescending(p =>
                p.Value.Position.AsBlockPos.HorizontalManhattenDistance(playerPos)),
            _ => waypoints
        };
    }
}