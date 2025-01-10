using System.Text;

namespace ApacheTech.VintageMods.CampaignCartographer.Features.FastTravelOverlay.MapLayer;

/// <summary>
///     Represents a map layer for the fast travel overlay on the world map. 
///     It handles rendering, mouse events, and rebuilding the map components 
///     related to the fast travel feature.
/// </summary>
[UsedImplicitly]
public class FastTravelOverlayMapLayer : MarkerMapLayer
{
    private readonly List<FastTravelOverlayMapComponent> _components = [];

    /// <summary>
    ///     The quad mesh model for rendering the map layer.
    /// </summary>
    public MeshRef QuadModel { get; } = ApiEx.Client.Render.UploadMesh(QuadMeshUtil.GetQuad());

    /// <summary>
    ///     The settings related to the fast travel overlay.
    /// </summary>
    public FastTravelOverlaySettings Settings { get; } = IOC.Services.GetRequiredService<FastTravelOverlaySettings>();

    /// <summary>
    ///     Initialises a new instance of the <see cref="FastTravelOverlayMapLayer"/> class.
    /// </summary>
    /// <param name="api">The core API instance.</param>
    /// <param name="mapSink">The world map manager to sink the map layer into.</param>
    public FastTravelOverlayMapLayer(ICoreAPI api, IWorldMapManager mapSink) : base(api, mapSink)
    {
    }

    /// <summary>
    ///     Called when the map is opened on the client. It rebuilds the map components related to the fast travel overlay.
    /// </summary>
    public override void OnMapOpenedClient()
        => RebuildMapComponents();

    /// <summary>
    ///     Renders the map layer components.
    /// </summary>
    /// <param name="mapElem">The map element to render the layer onto.</param>
    /// <param name="dt">The delta time for rendering, used for smooth animations.</param>
    public override void Render(GuiElementMap mapElem, float dt)
        => InvokeOnChildren(c => c.Render(mapElem, dt));

    /// <summary>
    ///     Handles mouse movement events over the map layer and triggers corresponding actions for map components.
    /// </summary>
    /// <param name="args">The mouse event arguments containing information about the movement.</param>
    /// <param name="mapElem">The map element being hovered over.</param>
    /// <param name="hoverText">The text to display when hovering over the map element.</param>
    public override void OnMouseMoveClient(MouseEvent args, GuiElementMap mapElem, StringBuilder hoverText)
        => InvokeMouseEventOnChildren(args, c => c.OnMouseMove(args, mapElem, hoverText));

    /// <summary>
    ///     Handles mouse button release events over the map layer and triggers corresponding actions for map components.
    /// </summary>
    /// <param name="args">The mouse event arguments containing information about the mouse button release.</param>
    /// <param name="mapElem">The map element being clicked.</param>
    public override void OnMouseUpClient(MouseEvent args, GuiElementMap mapElem)
        => InvokeMouseEventOnChildren(args, c => c.OnMouseUpOnElement(args, mapElem));

    /// <summary>
    ///     Indicates whether the layer requires chunks to be loaded before rendering. 
    ///     In this case, chunks are not required.
    /// </summary>
    public override bool RequireChunkLoaded => false;

    /// <summary>
    ///     The title of the map layer, displayed on the map interface.
    /// </summary>
    public override string Title => FastTravelOverlay.T("Title");

    /// <summary>
    ///     The group code for the map layer, used to identify and organise map layers.
    /// </summary>
    public override string LayerGroupCode => nameof(FastTravelOverlay);

    /// <summary>
    ///     The side of the map where the data is being displayed, indicating that it is client-side.
    /// </summary>
    public override EnumMapAppSide DataSide => EnumMapAppSide.Client;

    /// <summary>
    ///     Disposes of the map layer, including releasing the quad model to free resources.
    /// </summary>
    public override void Dispose()
    {
        QuadModel?.Dispose();
        base.Dispose();
    }

    /// <summary>
    ///     Rebuilds the map components based on the current fast travel overlay settings. 
    ///     This is called when the map is opened or when the settings change.
    /// </summary>
    public void RebuildMapComponents()
    {
        if (!mapSink.IsOpened) return;
        _components.Clear();
        foreach (var node in Settings.Nodes)
        {
            _components.Add(new FastTravelOverlayMapComponent(this, node));
        }
    }

    /// <summary>
    ///     Invokes a given action on all child map components (i.e., components within the fast travel overlay).
    /// </summary>
    /// <param name="action">The action to perform on each child component.</param>
    private void InvokeOnChildren(Action<FastTravelOverlayMapComponent> action)
    {
        if (!Active) return;
        foreach (var c in _components) action(c);
    }

    /// <summary>
    ///     Invokes a mouse event action on all child map components. If the event is handled, it stops further processing.
    /// </summary>
    /// <param name="args">The mouse event arguments to handle.</param>
    /// <param name="action">The action to perform on each child component based on the mouse event.</param>
    private void InvokeMouseEventOnChildren(MouseEvent args, Action<FastTravelOverlayMapComponent> action)
    {
        args.Handled = false;
        if (!Active) return;
        foreach (var c in _components)
        {
            action(c);
            if (args.Handled) return;
        }
    }
}