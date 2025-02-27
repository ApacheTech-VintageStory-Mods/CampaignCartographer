using ApacheTech.Common.Extensions.Harmony;
using ApacheTech.VintageMods.CampaignCartographer.Features.WaypointGroups.MapLayer;
using ApacheTech.VintageMods.CampaignCartographer.Features.WaypointGroups.Models;
using Gantry.Services.Brighter.Abstractions;

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
        public override AddWaypointGroupLayerCommand Handle(AddWaypointGroupLayerCommand command)
        {
            MapManager.RegisterMapLayer<WaypointGroupMapLayer>(command.Group.Id.ToString(), 100);
            var mapLayer = WaypointGroupMapLayer.Create(command.Group, MapManager);
            MapManager.MapLayers.Add(mapLayer);
            mapLayer.OnLoaded();
            return base.Handle(command);
        }
    }
}