using ApacheTech.VintageMods.CampaignCartographer.Features.WaypointManager.Extensions;
using ApacheTech.VintageMods.CampaignCartographer.Features.WaypointManager.Models;
using ApacheTech.VintageMods.CampaignCartographer.Features.WaypointManager.WaypointTemplates;
using Gantry.Core.Extensions.DotNet;
using Vintagestory.API.MathTools;

namespace ApacheTech.VintageMods.CampaignCartographer.Features.WaypointManager.Repositories;

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public class WaypointQueriesRepository
{
    private readonly WorldMapManager _mapManager;
    private WaypointMapLayer _mapLayer;

    public WaypointQueriesRepository(WorldMapManager mapManager)
    {
        _mapManager = mapManager;
    }

    public IEnumerable<KeyValuePair<int, Waypoint>> SortWaypoints(IEnumerable<PositionedWaypointTemplate> waypoints, WaypointSortType sortOrder)
    {
        var list = waypoints
            .Select(x => x.ToPositionedWaypoint())
            .ToList()
            .ToSortedDictionary();
        return Sort(sortOrder, list);
    }

    public IEnumerable<KeyValuePair<int, T>> GetSortedWaypoints<T>(WaypointSortType sortOrder, System.Func<Waypoint, T> mapper)
    {
        return GetSortedWaypoints(sortOrder)
            .Select(x => new KeyValuePair<int, T>(x.Key, mapper(x.Value)));
    }

    public IEnumerable<KeyValuePair<int, Waypoint>> GetSortedWaypoints(WaypointSortType sortOrder)
    {
        _mapLayer ??= _mapManager.WaypointMapLayer();
        var waypoints = _mapLayer.ownWaypoints.ToSortedDictionary();
        return Sort(sortOrder, waypoints);
    }

    public static IEnumerable<KeyValuePair<int, Waypoint>> Sort(WaypointSortType sortOrder, SortedDictionary<int, Waypoint> waypoints)
    {
        var playerPos = ApiEx.Client.World.Player.Entity.Pos.AsBlockPos;
        return sortOrder switch
        {
            WaypointSortType.IndexAscending => waypoints.Reverse(),
            WaypointSortType.IndexDescending => waypoints,
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