using System.Diagnostics.CodeAnalysis;
using Cairo;
using Gantry.Core.Extensions.Api;

// ReSharper disable StringLiteralTypo

namespace ApacheTech.VintageMods.CampaignCartographer.Features.WaypointGroups.Dialogue;

/// <summary>
///     A cell displayed within the cell list on the <see cref="WaypointGroupsDialogue"/> screen.
/// </summary>
/// <seealso cref="GuiElementTextBase" />
/// <seealso cref="IGuiElementCell" />
[SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Constant Values")]
public class WaypointGroupGuiCell : GuiElementTextBase, IGuiElementCell, IDisposable
{
    private LoadedTexture _cellTexture;
    private int _leftHighlightTextureId;

    /// <summary>
    ///     The cell entry associated with this GUI cell.
    /// </summary>
    public WaypointGroupCellEntry Cell { get; }

    /// <summary>
    ///     The bounds of this GUI cell.
    /// </summary>
    public new ElementBounds Bounds { get; }

    /// <summary>
    ///     The action to invoke when the cell is clicked.
    /// </summary>
    public required Action<int> OnMouseDownOnCell { private get; init; }

    /// <summary>
    ///     Initialises a new instance of the <see cref="WaypointGroupGuiCell" /> class.
    /// </summary>
    /// <param name="capi">The client API.</param>
    /// <param name="cell">The cell entry to display.</param>
    /// <param name="bounds">The bounds of the cell.</param>
    public WaypointGroupGuiCell(ICoreClientAPI capi, WaypointGroupCellEntry cell, ElementBounds bounds) : base(capi, "", null, bounds)
    {
        Cell = cell;
        _cellTexture = new LoadedTexture(capi);
        Cell.TitleFont ??= CairoFont.WhiteSmallText();
        Bounds = bounds.WithFixedHeight(30);
    }

    /// <summary>
    ///     Composes the cell's visual elements and textures.
    /// </summary>
    private void Compose()
    {
        ComposeHover(ref _leftHighlightTextureId);

        using var imageSurface = new ImageSurface(0, Bounds.OuterWidthInt, Bounds.OuterHeightInt);
        using var context = new Context(imageSurface);
        Bounds.CalcWorldBounds();

        // Form
        const double brightness = 1.2;
        RoundRectangle(context, 0.0, 0.0, Bounds.OuterWidth, Bounds.OuterHeight, 0.0);
        context.SetSourceRGBA(GuiStyle.DialogDefaultBgColor[0] * brightness, GuiStyle.DialogDefaultBgColor[1] * brightness, GuiStyle.DialogDefaultBgColor[2] * brightness, 1);
        context.Paint();

        // Main Title.
        Font = Cell.TitleFont;
        textUtil.AutobreakAndDrawMultilineTextAt(context, Font, Cell.Title, Bounds.absPaddingX, Bounds.absPaddingY + scaled(5), Bounds.InnerWidth);
        context.Operator = Operator.Add;
        EmbossRoundRectangleElement(context, 0.0, 0.0, Bounds.OuterWidth, Bounds.OuterHeight, false, 4, 0);
        context.SetSourceRGBA(0.0, 0.0, 0.0, 0.5);
        RoundRectangle(context, 0.0, 0.0, Bounds.OuterWidth, Bounds.OuterHeight, 1.0);
        context.Fill();
        generateTexture(imageSurface, ref _cellTexture);
    }

    /// <summary>
    ///     Composes the hover effect texture for the cell.
    /// </summary>
    /// <param name="textureId">The texture ID to update.</param>
    private void ComposeHover(ref int textureId)
    {
        var imageSurface = new ImageSurface(0, (int)Bounds.OuterWidth, (int)Bounds.OuterHeight);
        var context = genContext(imageSurface);

        context.NewPath();
        context.LineTo(0.0, 0.0);
        context.LineTo(Bounds.OuterWidth, 0.0);
        context.LineTo(Bounds.OuterWidth, Bounds.OuterHeight);
        context.LineTo(0.0, Bounds.OuterHeight);
        context.ClosePath();

        context.SetSourceRGBA(0.0, 0.0, 0.0, 0.15);
        context.Fill();
        generateTexture(imageSurface, ref textureId);
        context.Dispose();
        imageSurface.Dispose();
    }

    /// <summary>
    ///     Renders the interactive elements of the cell.
    /// </summary>
    /// <param name="capi">The client API.</param>
    /// <param name="deltaTime">The time since the last frame.</param>
    public void OnRenderInteractiveElements(ICoreClientAPI capi, float deltaTime)
    {
        if (_cellTexture.TextureId == 0) Compose();
        api.Render.Render2DTexturePremultipliedAlpha(_cellTexture.TextureId, (int)Bounds.absX, (int)Bounds.absY, Bounds.OuterWidthInt, Bounds.OuterHeightInt);
        var mouseX = api.Input.MouseX;
        var mouseY = api.Input.MouseY;
        var vec2d = Bounds.PositionInside(mouseX, mouseY);
        if (vec2d is not null)
        {
            api.Render.Render2DTexturePremultipliedAlpha(_leftHighlightTextureId, (int)Bounds.absX, (int)Bounds.absY, Bounds.OuterWidth, Bounds.OuterHeight);
        }
        api.Render.Render2DTexturePremultipliedAlpha(_leftHighlightTextureId, (int)Bounds.renderX, (int)Bounds.renderY, Bounds.OuterWidth, Bounds.OuterHeight);
    }

    /// <summary>
    ///     Updates the cell's height to a fixed value.
    /// </summary>
    public void UpdateCellHeight()
    {
        Bounds.fixedHeight = 30.0;
    }

    /// <summary>
    ///     Handles mouse down events on the cell element.
    /// </summary>
    /// <param name="args">The mouse event arguments.</param>
    /// <param name="elementIndex">The index of the element.</param>
    public void OnMouseDownOnElement(MouseEvent args, int elementIndex)
    {
    }

    /// <summary>
    ///     Handles mouse move events on the cell element.
    /// </summary>
    /// <param name="args">The mouse event arguments.</param>
    /// <param name="elementIndex">The index of the element.</param>
    public void OnMouseMoveOnElement(MouseEvent args, int elementIndex)
    {
    }

    /// <summary>
    ///     Handles mouse up events on the cell element and triggers the click action.
    /// </summary>
    /// <param name="args">The mouse event arguments.</param>
    /// <param name="elementIndex">The index of the element.</param>
    public void OnMouseUpOnElement(MouseEvent args, int elementIndex)
    {
        var mouseX = api.Input.MouseX;
        var mouseY = api.Input.MouseY;
        var vec2d = Bounds.PositionInside(mouseX, mouseY);
        api.Gui.PlaySound("menubutton_press");
        OnMouseDownOnCell?.Invoke(elementIndex);
        args.Handled = true;
    }

    /// <summary>
    ///     Disposes the resources used by the cell.
    /// </summary>
    public override void Dispose()
    {
        GC.SuppressFinalize(this);
        api.AsClientMain().EnqueueMainThreadTask(() =>
        {
            _cellTexture?.Dispose();
            if (_leftHighlightTextureId > 0)
                api.Render.GLDeleteTexture(_leftHighlightTextureId);
            base.Dispose();
        }, "");
    }
}