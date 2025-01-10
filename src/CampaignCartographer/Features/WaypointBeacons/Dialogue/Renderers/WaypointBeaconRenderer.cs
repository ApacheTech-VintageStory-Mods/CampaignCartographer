using System.Reflection.Emit;
using Gantry.Core.Extensions.Api;
using Gantry.Core.GameContent;
using Gantry.Core.Maths;
using Vintagestory.API.Config;
using Vintagestory.API.MathTools;

namespace ApacheTech.VintageMods.CampaignCartographer.Features.WaypointBeacons.Dialogue.Renderers;

/// <summary>
///     Handles the rendering of waypoint beacons in the game, including text and icons, 
///     and integrates with the HUD and GUI components.
/// </summary>
public class WaypointBeaconRenderer : IDisposable
{
    private readonly Matrixf _mvMat = new();
    private readonly WaypointBeaconHudElement _hudElement;
    private readonly WaypointBeaconViewModel _viewModel;
    private readonly WaypointBeaconPillarRenderer _pillarRenderer;
    private readonly ICoreClientAPI _capi;
    private GuiComposer _singleComposer;

    /// <summary>
    ///     Initialises a new instance of the <see cref="WaypointBeaconRenderer"/> class.
    /// </summary>
    /// <param name="hudElement">The HUD element associated with the waypoint beacon.</param>
    /// <param name="viewModel">The data model containing waypoint beacon information.</param>
    public WaypointBeaconRenderer(WaypointBeaconHudElement hudElement, WaypointBeaconViewModel viewModel)
    {
        _hudElement = hudElement;
        _viewModel = viewModel;
        _capi = ApiEx.Client;
        _pillarRenderer = new WaypointBeaconPillarRenderer(_hudElement, _viewModel);
        _capi.Event.RegisterRenderer(_pillarRenderer, EnumRenderStage.Opaque);
    }

    /// <summary>
    ///     Renders the waypoint beacon, including dynamic text and icons.
    /// </summary>
    public void Render()
    {
        _singleComposer ??= _hudElement.SingleComposer = Compose();
        if (_singleComposer is null) return;

        UpdateDynamicTextColour();
        var projectedPosition = ProjectWaypointPosition();
        var isClamped = TryClampPosition(projectedPosition);

        if (ShouldDisposeComposer(projectedPosition))
        {
            DisposeComposer();
            return;
        }

        UpdateComposerBounds(projectedPosition);
        UpdateDisplayText(isClamped);
        RenderWaypointIcon(isClamped);
    }

    /// <summary>
    ///     Attempt to clamp the icon to the edge of the screen, if the waypoint is not in the field of view.
    /// </summary>
    private bool TryClampPosition(Vec3d projectedPosition)
    {
        if (!_hudElement.Settings.ClampWaypointPosition) return false;

        double[] clamps =
        [
            _capi.Render.FrameWidth * 0.007,
            _capi.Render.FrameWidth * 1.001,
            _capi.Render.FrameHeight * -0.05,
            _capi.Render.FrameHeight * 0.938
        ];

        projectedPosition.X = GameMathsEx.TryClamp(projectedPosition.X, clamps[0], clamps[1], out var clampX);
        projectedPosition.Y = GameMathsEx.TryClamp(projectedPosition.Y, clamps[2], clamps[3], out var clampY);
        return clampX || clampY;
    }

    /// <summary>
    ///     Composes the GUI for the waypoint beacon text display.
    /// </summary>
    /// <returns>A new instance of <see cref="GuiComposer"/> configured for the beacon.</returns>
    private GuiComposer Compose()
    {
        var gapi = ApiEx.Client.Gui;
        var dialogBounds = ElementStdBounds.AutosizedMainDialogAtPos(0.0);
        var textBounds = ElementBounds.Fixed(EnumDialogArea.CenterMiddle, 0, 0, 250, 50);

        var font = CairoFont
            .WhiteSmallText()
            .WithColor(_viewModel.NormalisedColour)
            .WithStroke([0.0, 0.0, 0.0, 1.0], 1.0)
            .WithWeight(Cairo.FontWeight.Bold)
            .WithFontSize(15)
            .WithOrientation(EnumTextOrientation.Center);

        return gapi
            .CreateCompo($"WaypointBeacon-{_viewModel.Index}", dialogBounds)
            .AddDynamicText(_viewModel.Label, font, textBounds, "text")
            .ComposeHidden();
    }

    /// <summary>
    ///     Updates the text colour dynamically based on the waypoint settings.
    /// </summary>
    private void UpdateDynamicTextColour()
    {
        var dynamicText = _singleComposer.GetDynamicText("text");
        var colour = ColorUtil.ToRGBADoubles(_viewModel.Waypoint.Color);

        dynamicText.Font.Color = colour.With(c => c[3] = 1);
        dynamicText.Font.RenderTwice = true;
        dynamicText.Font.StrokeColor = [0, 0, 0, 1];
        dynamicText.Font.StrokeWidth = RuntimeEnv.GUIScale;
    }

    /// <summary>
    ///     Projects the waypoint's world position into screen coordinates.
    /// </summary>
    /// <returns>The screen position as a <see cref="Vec3d"/>.</returns>
    private Vec3d ProjectWaypointPosition()
    {
        return MatrixToolsd.Project(
            _viewModel.WaypointPosition,
            _capi.Render.PerspectiveProjectionMat,
            _capi.Render.PerspectiveViewMat,
            _capi.Render.FrameWidth,
            _capi.Render.FrameHeight);
    }

    /// <summary>
    ///     Determines whether the composer should be disposed based on position and distance.
    /// </summary>
    /// <param name="position">The screen position of the waypoint.</param>
    /// <returns><see langword="true"/> if the composer should be disposed; otherwise, <see langword="false"/>.</returns>
    private bool ShouldDisposeComposer(Vec3d position) =>
        position.Z < 0 || _viewModel.DistanceFromPlayer > _hudElement.Settings.IconRange;

    /// <summary>
    ///     Disposes of the composer resources and clears the display text.
    /// </summary>
    private void DisposeComposer()
    {
        _singleComposer.GetDynamicText("text").SetNewText("", forceRedraw: true);
        _singleComposer.Dispose();
        _hudElement.SingleComposer.Dispose();
    }

    /// <summary>
    ///     Updates the composer bounds based on the waypoint's projected position.
    /// </summary>
    /// <param name="position">The projected screen position.</param>
    private void UpdateComposerBounds(Vec3d position)
    {
        _singleComposer.Compose();
        _singleComposer.Bounds.absFixedX = position.X - _singleComposer.Bounds.OuterWidth / 2;
        _singleComposer.Bounds.absFixedY = _capi.Render.FrameHeight - position.Y - _singleComposer.Bounds.OuterHeight;
    }

    /// <summary>
    ///     Updates the display text for the waypoint beacon.
    /// </summary>
    private void UpdateDisplayText(bool isClamped)
    {
        var displayText = !isClamped && (_hudElement.IsAligned || _viewModel.DistanceFromPlayer < _hudElement.Settings.TitleRange);
        var dynamicText = _singleComposer.GetDynamicText("text");
        dynamicText.SetNewText(displayText ? _viewModel.Label : "", forceRedraw: true);
    }

    /// <summary>
    ///     Renders the waypoint icon.
    /// </summary>
    private void RenderWaypointIcon(bool isClamped)
    {
        var engineShader = _capi.Render.GetEngineShader(EnumShaderProgram.Gui);

        var newColour = new Vec4f();
        ColorUtil.ToRGBAVec4f(_viewModel.Waypoint.Color, ref newColour);
        AdjustColourForAlignment(ref newColour);

        ConfigureEngineShader(engineShader, newColour);

        if (!WaypointIconFactory.TryCreate(_viewModel.Waypoint.Icon, out var loadedTexture)) return;

        RenderIcon(engineShader, loadedTexture, isClamped);
    }

    /// <summary>
    ///     Adjusts the colour for alignment effects.
    /// </summary>
    /// <param name="colour">The colour to adjust.</param>
    private void AdjustColourForAlignment(ref Vec4f colour)
    {
        var h = _hudElement.IsAligned ? 2.0f : 1.0f;
        colour.Mul(new Vec4f(h, h, h, 1.0f));
    }

    /// <summary>
    ///     Configures the engine shader with specific rendering settings.
    /// </summary>
    /// <param name="engineShader">The shader program to configure.</param>
    /// <param name="colour">The colour to apply.</param>
    private static void ConfigureEngineShader(IShaderProgram engineShader, Vec4f colour)
    {
        engineShader.Uniform("rgbaIn", colour);
        engineShader.Uniform("extraGlow", 0);
        engineShader.Uniform("applyColor", 0);
        engineShader.Uniform("noTexture", 0.0f);
    }

    /// <summary>
    ///     Renders the waypoint icon using the specified shader and texture.
    /// </summary>
    /// <param name="engineShader">The shader program to use.</param>
    /// <param name="loadedTexture">The texture of the icon to render.</param>
    private void RenderIcon(IShaderProgram engineShader, LoadedTexture loadedTexture, bool isClamped)
    {
        var scale = isClamped || _hudElement.IsAligned ? 0.8f : 0.5f;

        engineShader.BindTexture2D("tex2d", loadedTexture.TextureId, 0);

        _mvMat.Set(_capi.Render.CurrentModelviewMatrix)
            .Translate(_singleComposer.Bounds.absFixedX + 125, _singleComposer.Bounds.absFixedY, _hudElement.ZSize)
            .Scale(loadedTexture.Width, loadedTexture.Height, 0.0f)
            .Scale(scale, scale, 0.0f);

        engineShader.UniformMatrix("projectionMatrix", _capi.Render.CurrentProjectionMatrix);
        engineShader.UniformMatrix("modelViewMatrix", _mvMat.Values);

        _capi.Render.RenderMesh(WaypointBeaconStore.QuadModel);
    }

    /// <summary>
    ///     Disposes of the renderer, including unregistering events and freeing resources.
    /// </summary>
    public void Dispose()
    {
        GC.SuppressFinalize(this);
        _capi.Event.AwaitMainThreadTask(() =>
        {
            _singleComposer?.Dispose();
            _hudElement.SingleComposer?.Dispose();
            _capi.Event.UnregisterRenderer(_pillarRenderer, EnumRenderStage.Opaque);
        });
    }
}