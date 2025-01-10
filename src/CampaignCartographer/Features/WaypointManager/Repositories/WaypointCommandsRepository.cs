using System.Threading.Tasks;
using ApacheTech.VintageMods.CampaignCartographer.Features.WaypointManager.Repositories.Commands;
using ApacheTech.VintageMods.CampaignCartographer.Features.WaypointManager.WaypointTemplates;
using Gantry.Core.Contracts;
using Vintagestory.API.MathTools;
using Vintagestory.API.Util;

// ReSharper disable ConvertToPrimaryConstructor

namespace ApacheTech.VintageMods.CampaignCartographer.Features.WaypointManager.Repositories;

/// <summary>
///     Provides methods to manage and process waypoints, including mass addition and removal,
///     with functionality to filter and execute waypoint-related commands.
/// </summary>
[HarmonyClientSidePatch]
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal class WaypointCommandsRepository
{
    private readonly WorldMapManager _mapManager;
    private static bool _processing;

    /// <summary>
    ///     Gets the list of saved waypoints from the map manager's waypoint layer.
    /// </summary>
    private List<Waypoint> SavedWaypoints => _mapManager?.WaypointMapLayer().ownWaypoints ?? [];

    /// <summary>
    ///     Handles the waypoint data received from the server, resetting the processing state.
    /// </summary>
    [HarmonyPostfix]
    [HarmonyPatch(typeof(WaypointMapLayer), nameof(WaypointMapLayer.OnDataFromServer))]
    private static void Patch_WaypointMapLayer_OnDataFromServer_Postfix()
    {
        _processing = false;
    }

    /// <summary>
    ///     Initialises a new instance of the <see cref="WaypointCommandsRepository"/> class.
    /// </summary>
    /// <param name="mapManager">The manager responsible for handling world map data.</param>
    public WaypointCommandsRepository(WorldMapManager mapManager)
    {
        _mapManager = mapManager;
    }

    /// <summary>
    ///     Filters the saved waypoints based on the provided predicate.
    /// </summary>
    /// <param name="filter">The predicate to filter the waypoints.</param>
    /// <returns>A collection of indices representing the filtered waypoints.</returns>
    private IEnumerable<int> Filter(System.Func<Waypoint, bool> filter)
    {
        var list = new List<int>();
        for (var i = 0; i < SavedWaypoints.Count; i++)
        {
            if (!filter(SavedWaypoints[i])) continue;
            list.Add(i);
        }
        list.Reverse();
        return list;
    }

    /// <summary>
    ///     Processes the mass removal of waypoints based on the provided filter.
    /// </summary>
    /// <param name="filter">The predicate to filter the waypoints for removal.</param>
    private void ProcessMassRemoval(System.Func<Waypoint, bool> filter)
    {
        var list = Filter(filter);
        var commands = new Queue<ICommand>();
        foreach (var waypoint in list) commands.Enqueue(new RemoveWaypointCommand(waypoint));
        _ = ProcessQueueAsync(commands);
    }

    /// <summary>
    ///     Processes the mass addition of waypoints.
    /// </summary>
    /// <param name="waypoints">The collection of waypoint templates to add.</param>
    private static void ProcessMassAddition(IEnumerable<PositionedWaypointTemplate> waypoints)
    {
        var commands = new Queue<ICommand>();
        foreach (var waypoint in waypoints)
            commands.Enqueue(new AddWaypointCommand(
                waypoint.Position.AsBlockPos,
                waypoint.ServerIcon,
                waypoint.Pinned,
                waypoint.Colour,
                waypoint.Title));
        _ = ProcessQueueAsync(commands);
    }

    /// <summary>
    ///     Executes a queue of commands asynchronously, ensuring thread safety for waypoint operations.
    /// </summary>
    /// <param name="commands">The queue of commands to process.</param>
    private static async Task ProcessQueueAsync(Queue<ICommand> commands)
    {
        WaypointManager.ShowFeedbackMessages = false;
        try
        {
            while (commands.Count > 0)
            {
                while (_processing) await Task.Delay(20);
                _processing = true;
                commands
                    .Dequeue()
                    .Execute();
            }
        }
        finally
        {
            await Task.Delay(1000);
            WaypointManager.ShowFeedbackMessages = true;
        }
    }

    /// <summary>
    ///     Removes all saved waypoints.
    /// </summary>
    public void RemoveAll()
    {
        ProcessMassRemoval(_ => true);
    }

    /// <summary>
    ///     Removes all waypoints located at a specific position.
    /// </summary>
    /// <param name="pos">The position to remove waypoints from.</param>
    public void RemoveAllWaypointsAtPosition(BlockPos pos)
    {
        ProcessMassRemoval(w => w.IsInHorizontalRangeOf(pos, 0));
    }

    /// <summary>
    ///     Removes all waypoints within a specified radius of the player's position.
    /// </summary>
    /// <param name="radius">The radius around the player within which to remove waypoints.</param>
    public void RemoveNearPlayer(int radius)
    {
        ProcessMassRemoval(w => w.IsInHorizontalRangeOf(
            ApiEx.Client.World.Player.Entity.Pos.AsBlockPos, radius));
    }

    /// <summary>
    ///     Removes all waypoints that have the specified icon.
    /// </summary>
    /// <param name="icon">The icon to match for removal.</param>
    public void RemoveByIcon(string icon)
    {
        ProcessMassRemoval(w => w.Icon == icon);
    }

    /// <summary>
    ///     Removes all waypoints that match the specified colour.
    /// </summary>
    /// <param name="colour">The colour to match for removal.</param>
    public void RemoveByColour(string colour)
    {
        ProcessMassRemoval(w => w.Color == colour.ToInt());
    }

    /// <summary>
    ///     Removes all waypoints whose titles start with the specified text.
    /// </summary>
    /// <param name="partialTitle">The partial title to match for removal.</param>
    public void RemoveByTitle(string partialTitle)
    {
        ProcessMassRemoval(w => w.Title.StartsWith(partialTitle, StringComparison.InvariantCultureIgnoreCase));
    }

    /// <summary>
    ///     Adds a collection of waypoints, avoiding duplicates.
    /// </summary>
    /// <typeparam name="T">The type of waypoint template.</typeparam>
    /// <param name="waypoints">The collection of waypoints to add.</param>
    public void AddWaypoints<T>(IEnumerable<T> waypoints) where T : PositionedWaypointTemplate
    {
        var currentIds = SavedWaypoints.Select(p => p.Guid);
        var newWaypoints = waypoints.Where(p => !currentIds.Contains(p.Id)).ToList();
        ProcessMassAddition(newWaypoints);
    }
}