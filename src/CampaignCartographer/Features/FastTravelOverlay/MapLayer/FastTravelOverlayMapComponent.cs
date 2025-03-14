using System.Text;
using ApacheTech.VintageMods.CampaignCartographer.Features.FastTravelOverlay.Dialogue;
using ApacheTech.VintageMods.CampaignCartographer.Features.FastTravelOverlay.Models;
using ApacheTech.VintageMods.CampaignCartographer.Features.TeleporterService;
using Gantry.Core.GameContent;
using Gantry.Core.Maths.Extensions;
using Gantry.Services.FileSystem.Configuration;
using Vintagestory.API.MathTools;
using Vintagestory.API.Util;
using static ApacheTech.VintageMods.CampaignCartographer.Features.FastTravelOverlay.FastTravelOverlay;

namespace ApacheTech.VintageMods.CampaignCartographer.Features.FastTravelOverlay.MapLayer;

/// <summary>
///     Represents the overlay map component for fast travel, handling rendering and interactions with fast travel nodes.
/// </summary>
public class FastTravelOverlayMapComponent : MapComponent
{
    private readonly Matrixf _modelViewMatrix = new();
    private readonly FastTravelOverlayNode _node;
    private readonly FastTravelOverlaySettings _settings;
    private readonly FastTravelOverlayMapLayer _mapLayer;

    private bool _hovering;
    private readonly TeleporterClientService _clientService;

    private string Colour => !_node.Enabled
        ? _settings.DisabledColour
        : _node.NodeColour.IfNullOrEmpty(_node.Type switch
        {
            FastTravelBlockType.Translocator => _settings.TranslocatorColour,
            FastTravelBlockType.Teleporter => _settings.TeleporterColour,
            _ => _settings.ErrorColour
        });

    private float Opacity => !_node.Enabled ? _settings.DisabledOpacity / 100f : 1f;

    /// <summary>
    ///     Initialises a new instance of the <see cref="FastTravelOverlayMapComponent"/> class.
    /// </summary>
    /// <param name="mapLayer">The map layer containing the fast travel settings.</param>
    /// <param name="node">The fast travel overlay node to be rendered and interacted with.</param>
    public FastTravelOverlayMapComponent(FastTravelOverlayMapLayer mapLayer, FastTravelOverlayNode node)
        : base(ApiEx.Client)
    {
        _node = node;
        _mapLayer = mapLayer;
        _settings = mapLayer.Settings;
        _clientService = IOC.Services.GetRequiredService<TeleporterClientService>();
    }

    /// <inheritdoc />
    public override void OnMouseUpOnElement(MouseEvent args, GuiElementMap mapElem)
    {
        args.Handled = false;
        if (args.Button != EnumMouseButton.Right) return;
        if (!_hovering) return;
        capi.Event.EnqueueMainThreadTask(() =>
        {
            var dialogue = FastTravelOverlayNodeDialogue.Edit(_node);
            dialogue.ToggleGui();
        }, "");
        args.Handled = true;
    }

    /// <inheritdoc />
    public override void Render(GuiElementMap map, float dt)
    {
        var here = new Vec2f();
        map.TranslateWorldPosToViewPos(_node.Location.SourcePos.ToVec3d(), ref here);

        var there = new Vec2f();
        if (_node.Location.TargetPos is not null)
        {
            map.TranslateWorldPosToViewPos(_node.Location.TargetPos.ToVec3d(), ref there);
        }
        else
        {
            if (!TryFindDestination(out var targetPosition))
            {
                RenderEndpointNode(map, here, there);
                return;
            }
            _node.Location.TargetPos = targetPosition.AsBlockPos;
            ModSettings.World.Save(_settings);
            map.TranslateWorldPosToViewPos(targetPosition, ref there);
        }

        RenderEndpointNode(map, here, there);
        RenderEndpointNode(map, there, here);
        if (!_node.ShowPath) return;
        RenderPath(map, here, there);
    }

    /// <summary>
    ///     Renders the path between two fast travel nodes on the map.
    /// </summary>
    /// <param name="map">The map on which to render the path.</param>
    /// <param name="here">The screen coordinates of the origin node.</param>
    /// <param name="there">The screen coordinates of the destination node.</param>
    private void RenderPath(GuiElementMap map, Vec2f here, Vec2f there)
    {
        // Convert coordinates to render positions
        var x1 = (float)(map.Bounds.renderX + here.X);
        var y1 = (float)(map.Bounds.renderY + here.Y);
        var x2 = (float)(map.Bounds.renderX + there.X);
        var y2 = (float)(map.Bounds.renderY + there.Y);

        var api = map.Api;
        var colour = new Vec4f();
        ColorUtil.ToRGBAVec4f(Colour.ColourValue(), ref colour); // Assuming `_colour` is defined elsewhere
        colour.A = Opacity;

        // Set up the shader program
        var prog = api.Render.GetEngineShader(EnumShaderProgram.Gui);
        prog.Uniform("rgbaIn", colour);
        prog.Uniform("extraGlow", 0);
        prog.Uniform("applyColor", 0);
        prog.Uniform("noTexture", 1f); // No texture is used for the line
        prog.UniformMatrix("projectionMatrix", api.Render.CurrentProjectionMatrix);

        // Use a custom model-view matrix for rendering the line
        _modelViewMatrix.Set(api.Render.CurrentModelviewMatrix);
        var dx = x2 - x1;
        var dy = y2 - y1;
        var length = MathF.Sqrt(dx * dx + dy * dy) / 2;

        var hover = _hovering ? 1 : 1.1f;
        // Correct the matrix to avoid extending the line in the opposite direction
        _modelViewMatrix
            .Translate(x1, y1, 60f) // Start at A (x1, y1)
            .RotateZ(MathF.Atan2(dy, dx)) // Rotate towards B
            .Translate(length, 0, 0) // Offset the quad to prevent extending in the opposite direction
            .Scale(length, _settings.PathWidth * hover, 1f); // Scale only in the direction of the line

        prog.UniformMatrix("modelViewMatrix", _modelViewMatrix.Values);

        // Render the quad stretched as a line
        api.Render.RenderMesh(_mapLayer.QuadModel); // Assuming a basic quad model is used for the line
    }

    /// <summary>
    ///     Renders the endpoint node at a specified position on the map.
    /// </summary>
    /// <param name="map">The map on which to render the node.</param>
    /// <param name="here">The screen coordinates of the node.</param>
    /// <param name="there">The screen coordinates of the destination node (optional).</param>
    private void RenderEndpointNode(GuiElementMap map, Vec2f here, Vec2f there = null)
    {
        if (here.X < -10f || here.Y < -10f ||
            here.X > map.Bounds.OuterWidth + 10.0 || here.Y > map.Bounds.OuterHeight + 10.0)
            return;

        var x = (float)(map.Bounds.renderX + here.X);
        var y = (float)(map.Bounds.renderY + here.Y);
        var api = map.Api;
        var colour = new Vec4f();
        var nodeColour = there is null ? _settings.ErrorColour : Colour;
        var nodeOpacity = there is null ? _settings.ErrorOpacity : Opacity;
        ColorUtil.ToRGBAVec4f(nodeColour.ColourValue(), ref colour);
        colour.A = nodeOpacity;
        var prog = api.Render.GetEngineShader(EnumShaderProgram.Gui);
        prog.Uniform("rgbaIn", colour);
        prog.Uniform("extraGlow", 0);
        prog.Uniform("applyColor", 0);
        prog.Uniform("noTexture", 0f);

        var hover = (_hovering ? 6 : 0) - 1.5f * Math.Max(1f, 1f / map.ZoomLevel);
        if (!WaypointIconFactory.TryCreate("circle", out var texture)) return;

        var scale = (4 + _settings.NodeSize) / 20f;

        prog.BindTexture2D("tex2d", texture.TextureId, 0);
        prog.UniformMatrix("projectionMatrix", api.Render.CurrentProjectionMatrix);
        _modelViewMatrix
            .Set(api.Render.CurrentModelviewMatrix)
            .Translate(x, y, 60f)
            .Scale(texture.Width + hover, texture.Height + hover, 0f)
            .Scale(scale, scale, 0f);

        var shadowMvMat = _modelViewMatrix.Clone().Scale(1.25f, 1.25f, 1.25f);
        prog.Uniform("rgbaIn", new Vec4f(0f, 0f, 0f, 0.6f));
        prog.UniformMatrix("modelViewMatrix", shadowMvMat.Values);

        api.Render.RenderMesh(_mapLayer.QuadModel);
        prog.Uniform("rgbaIn", colour);
        prog.UniformMatrix("modelViewMatrix", _modelViewMatrix.Values);
        api.Render.RenderMesh(_mapLayer.QuadModel);
    }

    /// <inheritdoc />
    public override void OnMouseMove(MouseEvent args, GuiElementMap mapElem, StringBuilder hoverText)
    {
        var originHover = IsHovering(args, mapElem, _node.Location.SourcePos);
        var targetHover = IsHovering(args, mapElem, _node.Location.TargetPos);

        _hovering = originHover || targetHover;
        if (!_hovering)
        {
            args.Handled = false;
            return;
        }

        hoverText.Append(GetHoverText(originHover));

        args.Handled = true;
    }

    /// <summary>
    ///     Gets the title of a fast travel node, based on its source or target name.
    /// </summary>
    /// <param name="endpointName">The name of the endpoint (either source or target).</param>
    /// <returns>The title of the fast travel node.</returns>
    private string GetTitle(string endpointName)
    {
        var title = _node.Location.SourceName;
        return title != endpointName ? title : endpointName;
    }

    /// <summary>
    ///     Gets the hover text displayed when the user hovers over a fast travel node.
    /// </summary>
    /// <param name="originHover">A flag indicating whether the hover is over the origin or target node.</param>
    /// <returns>The hover text to display.</returns>
    private string GetHoverText(bool originHover)
    {
        var hoverText = new StringBuilder();

        var type = T($"FastTravelBlockType.{_node.Type}").UcFirst();
        var title = originHover ? GetTitle(_node.Location.SourceName) : _node.Location.TargetName.IfNullOrEmpty(T("DefaultTitle"));
        hoverText.AppendLine($"{type}: {title}");

        if (_node.Location.TargetPos is not null)
        {
            var targetPosition = (originHover ? _node.Location.TargetPos : _node.Location.SourcePos).ToRelativeCoordinateString();
            hoverText.AppendLine(T("HoverText.Target", targetPosition));
        }

        return hoverText.ToString();
    }

    /// <summary>
    ///     Determines if the mouse is hovering over the specified position on the map.
    /// </summary>
    /// <param name="args">The mouse event arguments.</param>
    /// <param name="mapElem">The map element.</param>
    /// <param name="worldPos">The world position to check.</param>
    /// <returns><c>true</c> if the mouse is hovering over the position; otherwise, <c>false</c>.</returns>
    private static bool IsHovering(MouseEvent args, GuiElementMap mapElem, BlockPos worldPos)
    {
        if (worldPos is null) return false;
        var viewPos = new Vec2f();
        mapElem.TranslateWorldPosToViewPos(worldPos.ToVec3d(), ref viewPos);
        var x = viewPos.X + mapElem.Bounds.renderX;
        var y = viewPos.Y + mapElem.Bounds.renderY;
        var dX = args.X - x;
        var dY = args.Y - y;
        var size = RuntimeEnv.GUIScale * 8f;
        return Math.Abs(dX) < size && Math.Abs(dY) < size;
    }

    /// <summary>
    ///     Attempts to find the destination position for a fast travel node.
    /// </summary>
    /// <param name="worldPos">The destination world position if found.</param>
    /// <returns><c>true</c> if the destination was found; otherwise, <c>false</c>.</returns>
    private bool TryFindDestination(out Vec3d worldPos)
    {
        worldPos = null;
        try
        {
            var blockAccessor = ApiEx.ClientMain.BlockAccessor;
            var blockEntity = blockAccessor.GetBlockEntity(_node.Location.SourcePos);
            switch (blockEntity)
            {
                case BlockEntityStaticTranslocator translocator:
                    {
                        if (!translocator.FullyRepaired) return false;
                        if (translocator.TargetLocation is null) return false;
                        translocator.TargetLocation.ToVec3d();
                        return worldPos is not null;
                    }
                case BlockEntityTeleporter:
                    {
                        var location = _clientService.TeleporterLocations.FirstOrDefault(p => p.SourcePos == _node.Location.SourcePos);
                        worldPos = location?.TargetPos?.ToVec3d();
                        return worldPos is not null;
                    }
                default:
                    return false;
            }
        }
        catch (Exception ex)
        {
            G.Logger.Error("Could not find destination for fast travel node.", ex);
            return false;
        }
    }
}