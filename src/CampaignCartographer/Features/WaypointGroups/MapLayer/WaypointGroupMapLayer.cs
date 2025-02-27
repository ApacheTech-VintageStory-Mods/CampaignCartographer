using System.Text;
using ApacheTech.VintageMods.CampaignCartographer.Features.WaypointGroups.Models;

namespace ApacheTech.VintageMods.CampaignCartographer.Features.WaypointGroups.MapLayer;

public class WaypointGroupMapLayer(ICoreAPI api, IWorldMapManager mapSink) : WaypointMapLayer(api, mapSink)
{
    private WaypointGroup _group;
    private readonly List<WaypointMapComponent> _waypointMapComponents = [];

    public static WaypointGroupMapLayer Create(WaypointGroup group, IWorldMapManager mapSink)
    {
        var mapLayer = new WaypointGroupMapLayer(ApiEx.Client, mapSink);
        mapLayer.SetGroup(group);
        return mapLayer;
    }

    /// <summary>
    ///     Sets the waypoint group.
    /// </summary>
    /// <param name="group">The waypoint group to set.</param>
    private void SetGroup(WaypointGroup group) => _group = group;

    /// <summary>
    ///     Updates the title of the waypoint group.
    /// </summary>
    /// <param name="title">The new title to assign to the waypoint group.</param>
    public void UpdateTitle(string title) => _group.Title = title;

    /// <summary>
    ///     Updates the list of waypoints associated with the waypoint group.
    /// </summary>
    /// <param name="waypoints">The new list of waypoint identifiers.</param>
    public void UpdateWaypoints(List<Guid> waypoints) => _group.Waypoints = waypoints;

    /// <summary>
    ///     Clears all waypoint map components from the group.
    /// </summary>
    public void ClearComponents() => _waypointMapComponents.Clear();

    /// <summary>
    ///     Adds a waypoint map component to the group if it is not already present.
    /// </summary>
    /// <param name="component">The waypoint map component to add.</param>
    public void AddComponent(WaypointMapComponent component) => _waypointMapComponents.AddIfNotPresent(component);

    /// <summary>
    ///     Removes a waypoint map component from the group if it is present.
    /// </summary>
    /// <param name="component">The waypoint map component to remove.</param>
    public void RemoveComponent(WaypointMapComponent component) => _waypointMapComponents.RemoveIfPresent(component);

    #region Base Overrides

    /// <inheritdoc />
    public override string Title => _group?.Title.IfNullOrEmpty(string.Empty);

    /// <inheritdoc />
    public override string LayerGroupCode => _group?.Id.ToString().IfNullOrEmpty(string.Empty);

    /// <inheritdoc />
    public override EnumMapAppSide DataSide => EnumMapAppSide.Client;

    /// <inheritdoc />
    public override bool RequireChunkLoaded => false;

    /// <inheritdoc />
    public override void Render(GuiElementMap mapElem, float dt)
    {
        if (!Active) return;
        foreach (var mapComponent in _waypointMapComponents)
        {
            mapComponent.Render(mapElem, dt);
        }
    }

    /// <inheritdoc />
    public override void OnMouseUpClient(MouseEvent args, GuiElementMap mapElem)
    {
        if (!Active) return;
        foreach (var mapComponent in _waypointMapComponents)
        {
            mapComponent.OnMouseUpOnElement(args, mapElem);
            if (args.Handled) break;
        }
    }

    /// <inheritdoc />
    public override void OnMouseMoveClient(MouseEvent args, GuiElementMap mapElem, StringBuilder hoverText)
    {
        if (!Active) return;
        foreach (var mapComponent in _waypointMapComponents)
        {
            mapComponent.OnMouseMove(args, mapElem, hoverText);
        }
    }

    #endregion
}