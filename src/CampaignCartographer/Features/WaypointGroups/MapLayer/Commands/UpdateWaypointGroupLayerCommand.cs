using ApacheTech.VintageMods.CampaignCartographer.Features.WaypointGroups.Models;
using Gantry.Services.Brighter.Abstractions;
using Gantry.Services.Brighter.Filters;

namespace ApacheTech.VintageMods.CampaignCartographer.Features.WaypointGroups.MapLayer.Commands;

/// <summary>
///     Represents a command to update a waypoint group layer on the map.
/// </summary>
public class UpdateWaypointGroupLayerCommand : CommandBase
{
    /// <summary>
    ///     The waypoint group containing the updated information.
    /// </summary>
    public WaypointGroup Group { get; set; }

    /// <summary>
    ///     Handles the execution of the <see cref="UpdateWaypointGroupLayerCommand"/>.
    /// </summary>
    private class UpdateWaypointGroupLayerHandler(WorldMapManager mapManager)
        : WorldMapManagerRequestHandler<UpdateWaypointGroupLayerCommand>(mapManager)
    {
        /// <inheritdoc />
        [HandledOnClient]
        public override UpdateWaypointGroupLayerCommand Handle(UpdateWaypointGroupLayerCommand command)
        {
            var mapLayer = MapManager.MapLayers
                .SingleOrDefault(p => p.LayerGroupCode == command.Group.Id.ToString())?
                .To<WaypointGroupMapLayer>();

            mapLayer?.UpdateTitle(command.Group.Title);
            mapLayer?.UpdateWaypoints(command.Group.Waypoints);

            ApiEx.Logger.VerboseDebug($"Waypoint group with id {command.Group.Id} updated map with name '{command.Group.Title}'.");
            return base.Handle(command);
        }
    }
}