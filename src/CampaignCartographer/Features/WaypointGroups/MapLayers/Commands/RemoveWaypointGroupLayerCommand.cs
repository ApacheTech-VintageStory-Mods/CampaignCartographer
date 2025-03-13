using ApacheTech.VintageMods.CampaignCartographer.Features.WaypointGroups.Abstractions;
using ApacheTech.VintageMods.CampaignCartographer.Features.WaypointGroups.Extensions;
using Gantry.Services.Brighter.Abstractions;

namespace ApacheTech.VintageMods.CampaignCartographer.Features.WaypointGroups.MapLayers.Commands;

/// <summary>
///     Represents a command to remove a waypoint group layer from the map.
/// </summary>
public class RemoveWaypointGroupLayerCommand : CommandBase
{
    /// <summary>
    ///     The identifier of the waypoint group to be removed.
    /// </summary>
    public string GroupId { get; set; }

    /// <summary>
    ///     Handles the execution of the <see cref="RemoveWaypointGroupLayerCommand"/>.
    /// </summary>
    private class RemoveWaypointGroupLayerHandler(WorldMapManager mapManager)
        : WorldMapManagerRequestHandler<RemoveWaypointGroupLayerCommand>(mapManager)
    {
        /// <inheritdoc />
        [LockMapLayerGeneration]
        public override RemoveWaypointGroupLayerCommand Handle(RemoveWaypointGroupLayerCommand command)
        {
            MapManager.UnregisterMapLayer(command.GroupId);

            G.Log.VerboseDebug($"Waypoint group with id {command.GroupId} removed from map.");
            return base.Handle(command);
        }
    }
}