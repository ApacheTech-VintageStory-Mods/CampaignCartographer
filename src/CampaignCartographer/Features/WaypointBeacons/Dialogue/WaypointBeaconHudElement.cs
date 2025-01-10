using ApacheTech.VintageMods.CampaignCartographer.Features.WaypointBeacons.Dialogue.Renderers;
using ApacheTech.VintageMods.CampaignCartographer.Features.WaypointManager.Dialogues;
using Gantry.Core.Annotation;

namespace ApacheTech.VintageMods.CampaignCartographer.Features.WaypointBeacons.Dialogue;

/// <summary>
///     Represents a waypoint beacon HUD element that displays waypoint information in the UI.
/// </summary>
[ClientSide]
public class WaypointBeaconHudElement : HudElement
{
    private readonly WaypointBeaconViewModel _viewModel;
    private readonly WaypointBeaconRenderer _renderer;

    /// <summary>
    ///     Initialises a new instance of the <see cref="WaypointBeaconHudElement"/> class.
    /// </summary>
    /// <param name="capi">The core client API used to interact with the game world and client-side features.</param>
    /// <param name="waypointGuid">The ID of the waypoint this beacon tracks.</param>
    public WaypointBeaconHudElement(ICoreClientAPI capi, string waypointGuid) : base(capi)
    {
        Settings = IOC.Services.GetRequiredService<WaypointBeaconsSettings>();
        _viewModel = new WaypointBeaconViewModel(waypointGuid, Settings);
        _renderer = new WaypointBeaconRenderer(this, _viewModel);
    }

    /// <summary>
    ///     The settings for waypoint beacons.
    /// </summary>
    public WaypointBeaconsSettings Settings { get; }

    /// <summary>
    ///     A value indicating whether the waypoint can be closed.
    /// </summary>
    public bool Closeable => IsOpened() && !_viewModel.Visible;

    /// <summary>
    ///     A value indicating whether the waypoint can be opened.
    /// </summary>
    public bool Openable => !IsOpened() && _viewModel.Visible;

    /// <summary>
    ///     Indicates whether the pillar is aligned to the waypoint.
    /// </summary>
    public bool IsAligned
    {
        get
        {
            var xPos = (double)capi.Input.MouseX / capi.Render.FrameWidth;
            var yPos = (double)capi.Input.MouseY / capi.Render.FrameHeight;
            var fX = (SingleComposer.Bounds.absFixedX + SingleComposer.Bounds.InnerWidth / 2) / capi.Render.FrameWidth;
            var fY = SingleComposer.Bounds.absFixedY / capi.Render.FrameHeight - 0.01;
            return xPos < fX + 0.01 && xPos > fX - 0.01 && yPos < fY + 0.01 && yPos > fY - 0.01;
        }
    }

    /// <summary>
    ///     Opens the waypoint edit dialogue and focuses on it.
    /// </summary>
    public void OpenEditDialogue()
    {
        var dialogue = new AddEditWaypointDialogue(ApiEx.Client, _viewModel.Waypoint, _viewModel.Index);
        dialogue.ToggleGui();
    }

    /// <summary>
    ///     Renders the GUI elements for this HUD element.
    /// </summary>
    /// <param name="deltaTime">The time elapsed since the last frame, in seconds.</param>
    public override void OnRenderGUI(float deltaTime)
    {
        if (!IsOpened() || !_viewModel.Available) return;
        _renderer.Render();
        base.OnRenderGUI(deltaTime);
    }

    /// <inherit cref="GuiDialog.DrawOrder" />
    public override double DrawOrder => -0.11;

    /// <summary>
    ///     Determines whether the element should receive mouse events.
    /// </summary>
    /// <returns>False as the element does not receive mouse events.</returns>
    public override bool ShouldReceiveMouseEvents() => false;

    /// <summary>
    ///     The size of the element along the Z-axis.
    /// </summary>
    public override float ZSize => 0.00001f;

    /// <summary>
    ///     Indicates whether the element is focused.
    /// </summary>
    public override bool Focused => false;

    /// <summary>
    ///     Disposes of the resources used by the element, including unregistering renderers and disposing dialogues.
    /// </summary>
    public override void Dispose()
    {
        GC.SuppressFinalize(this);
        _renderer.Dispose();
        base.Dispose();
    }
}