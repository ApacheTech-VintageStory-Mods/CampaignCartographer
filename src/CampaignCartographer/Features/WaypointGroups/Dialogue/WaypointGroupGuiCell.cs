using System.Diagnostics.CodeAnalysis;
using Cairo;

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

    public WaypointGroupCellEntry Cell { get; }

    public new ElementBounds Bounds { get; }

    public Action<int> OnMouseDownOnCell { private get; init; }

    /// <summary>
    /// 	Initialises a new instance of the <see cref="WaypointGroupGuiCell" /> class.
    /// </summary>
    public WaypointGroupGuiCell(ICoreClientAPI capi, WaypointGroupCellEntry cell, ElementBounds bounds) : base(capi, "", null, bounds)
    {
        Cell = cell;
        _cellTexture = new LoadedTexture(capi);
        Cell.TitleFont ??= CairoFont.WhiteSmallText();
        Bounds = bounds.WithFixedHeight(30);
    }

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

    public void UpdateCellHeight()
    {
        Bounds.fixedHeight = 30.0;
    }

    public void OnMouseDownOnElement(MouseEvent args, int elementIndex)
    {
    }

    public void OnMouseMoveOnElement(MouseEvent args, int elementIndex)
    {
    }

    public void OnMouseUpOnElement(MouseEvent args, int elementIndex)
    {
        var mouseX = api.Input.MouseX;
        var mouseY = api.Input.MouseY;
        var vec2d = Bounds.PositionInside(mouseX, mouseY);
        api.Gui.PlaySound("menubutton_press");
        OnMouseDownOnCell?.Invoke(elementIndex);
        args.Handled = true;
    }

    public override void Dispose()
    {
        GC.SuppressFinalize(this);
        ApiEx.ClientMain.EnqueueMainThreadTask(() =>
        {
            _cellTexture?.Dispose();
            api.Render.GLDeleteTexture(_leftHighlightTextureId);
            base.Dispose();
        }, "");
    }
}