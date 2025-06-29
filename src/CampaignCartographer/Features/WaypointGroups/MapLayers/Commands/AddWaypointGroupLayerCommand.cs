using ApacheTech.VintageMods.CampaignCartographer.Features.WaypointGroups.Abstractions;
using ApacheTech.VintageMods.CampaignCartographer.Features.WaypointGroups.Models;
using Gantry.Services.Brighter.Abstractions;

namespace ApacheTech.VintageMods.CampaignCartographer.Features.WaypointGroups.MapLayers.Commands;

/// <summary>
///     Represents a command to add a waypoint group as a map layer.
/// </summary>
public class AddWaypointGroupLayerCommand : CommandBase
{
    /// <summary>
    ///     The waypoint group to be added as a layer.
    /// </summary>
    public required WaypointGroup Group { get; init; }

    /// <summary>
    ///     Handles the execution of the <see cref="AddWaypointGroupLayerCommand"/>.
    /// </summary>
    private class AddWaypointGroupLayerHandler(WorldMapManager mapManager, ICoreClientAPI capi)
        : WorldMapManagerRequestHandler<AddWaypointGroupLayerCommand>(mapManager, capi)
    {
        /// <inheritdoc />
        [LockMapLayerGeneration]
        public override AddWaypointGroupLayerCommand Handle(AddWaypointGroupLayerCommand command)
        {
            MapManager.RegisterMapLayer<WaypointGroupMapLayer>(command.Group.Id.ToString(), 100);
            var mapLayer = WaypointGroupMapLayer.Create(command.Group, MapManager);
            MapManager.MapLayers.Add(mapLayer);
            mapLayer.OnLoaded();
            G.Log($"Waypoint group with id {command.Group.Id} added to map with name '{command.Group.Title}'.");
            return base.Handle(command);
        }
    }
}