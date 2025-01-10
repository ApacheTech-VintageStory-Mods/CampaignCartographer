using Vintagestory.API.MathTools;

namespace ApacheTech.VintageMods.CampaignCartographer.Features.WaypointBeacons.Dialogue.Renderers;

/// <summary>
///     Renderer for the pillar associated with a <see cref="WaypointBeaconHudElement"/>.
/// </summary>
/// <param name="element">The floaty waypoint element associated with this renderer.</param>
internal class WaypointBeaconPillarRenderer(WaypointBeaconHudElement element, WaypointBeaconViewModel viewModel) : IRenderer
{
    private readonly WaypointBeaconHudElement _element = element;
    private readonly WaypointBeaconViewModel _viewModel = viewModel;

    private readonly Matrixf _mvMat = new();
    private float _counter;

    /// <summary>
    ///     The render order for the pillar renderer.
    /// </summary>
    /// <remarks>
    ///     This value determines when the pillar will be rendered relative to other objects.
    /// </remarks>
    public double RenderOrder => 0.5;

    /// <summary>
    ///     The render range for the pillar renderer.
    /// </summary>
    /// <remarks>
    ///     This value specifies the maximum distance at which the pillar will be rendered.
    /// </remarks>
    public int RenderRange => 24;

    /// <summary>
    ///     Renders the frame for the pillar, applying various transformations and settings.
    /// </summary>
    /// <param name="deltaTime">The time elapsed since the last frame, used for animations and frame-based updates.</param>
    /// <param name="stage">The current rendering stage (opaque or transparent).</param>
    public void OnRenderFrame(float deltaTime, EnumRenderStage stage)
    {
        if (!_viewModel.Available) return;
        if (!_element.Settings.ShowPillars) return;
        if (!_element.IsOpened()) return;
        if (!_viewModel.Visible) return;
        var capi = ApiEx.Client;
        if (capi.HideGuis) return;

        _counter += deltaTime;
        var pos = _viewModel.WaypointPosition;
        var cameraPos = capi.World.Player.Entity.CameraPos;
        var prog = capi.Render.PreparedStandardShader((int)pos.X, (int)pos.Y, (int)pos.Z);

        capi.Render.GlToggleBlend(true);
        var newColour = new Vec4f();
        ColorUtil.ToRGBAVec4f(_viewModel.Waypoint.Color, ref newColour);
        var h = _element.IsAligned ? 2.0f : 1.0f;
        newColour.Mul(new Vec4f(h, h, h, 1.0f));
        newColour.A = 0.9f;

        prog.RgbaTint = newColour;
        prog.RgbaGlowIn = newColour;
        prog.NormalShaded = 0;
        prog.ExtraGlow = 64;

        var distanceFromPlayer = _viewModel.Waypoint.DistanceFromPlayer();
        var scale = (float)GameMath.Max(distanceFromPlayer / ClientSettings.FieldOfView, 1.0f);
        prog.Tex2D = capi.Render.GetOrLoadTexture(new AssetLocation("block/creative/col78.png"));
        prog.ModelMatrix = _mvMat
            .Identity()
            .Translate(pos.X - cameraPos.X, pos.Y - cameraPos.Y, pos.Z - cameraPos.Z)
            .Scale(scale, scale, scale)
            .RotateYDeg(_counter * 50 % 360.0f).Values;
        prog.ViewMatrix = capi.Render.CameraMatrixOriginf;
        prog.ProjectionMatrix = capi.Render.CurrentProjectionMatrix;
        //prog.BindTexture2D("tex", 0, 0);

        capi.Render.RenderMesh(WaypointBeaconStore.PillarMeshRef);

        prog.Stop();
    }

    /// <summary>
    ///     Disposes of the resources used by the renderer.
    /// </summary>
    public void Dispose()
        => GC.SuppressFinalize(this);
}