using ApacheTech.VintageMods.CampaignCartographer.Features.WaypointGroups.Models;
using Gantry.Services.Brighter.Abstractions;
using Gantry.Services.Brighter.Filters;

namespace ApacheTech.VintageMods.CampaignCartographer.Features.WaypointGroups.MapLayer.Commands;

/// <summary>
///     Represents a command to add a waypoint group as a map layer.
/// </summary>
public class AddWaypointGroupLayerCommand : CommandBase
{
    /// <summary>
    ///     The waypoint group to be added as a layer.
    /// </summary>
    public WaypointGroup Group { get; init; }

    /// <summary>
    ///     Handles the execution of the <see cref="AddWaypointGroupLayerCommand"/>.
    /// </summary>
    private class AddWaypointGroupLayerHandler(WorldMapManager mapManager)
        : WorldMapManagerRequestHandler<AddWaypointGroupLayerCommand>(mapManager)
    {
        /// <inheritdoc />
        [HandledOnClient]
        public override AddWaypointGroupLayerCommand Handle(AddWaypointGroupLayerCommand command)
        {
            MapManager.RegisterMapLayer<WaypointGroupMapLayer>(command.Group.Id.ToString(), 100);
            var mapLayer = WaypointGroupMapLayer.Create(command.Group, MapManager);
            MapManager.MapLayers.Add(mapLayer);
            mapLayer.OnLoaded();

            ApiEx.Logger.VerboseDebug($"Waypoint group with id {command.Group.Id} added to map with name '{command.Group.Title}'.");
            return base.Handle(command);
        }
    }
}