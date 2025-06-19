using ApacheTech.VintageMods.CampaignCartographer.Features.FastTravelOverlay.MapLayer;
using ApacheTech.VintageMods.CampaignCartographer.Features.FastTravelOverlay.Models;
using Cairo;
using Gantry.Core.GameContent.AssetEnum;
using Gantry.Core.GameContent.GUI.Abstractions;
using Gantry.Core.GameContent.GUI.Models;
using Gantry.Services.FileSystem.Configuration;
using Vintagestory.API.MathTools;
using static ApacheTech.VintageMods.CampaignCartographer.Features.FastTravelOverlay.FastTravelOverlay;

namespace ApacheTech.VintageMods.CampaignCartographer.Features.FastTravelOverlay.Dialogue;

public class FastTravelOverlayNodeDialogue : GenericDialogue
{
    private readonly CrudAction _mode;
    private readonly FastTravelOverlayNode _node;
    private readonly FastTravelOverlayNode _original;
    private readonly FastTravelOverlaySettings _fastTravelOverlaySettings;

    private string BlockType => T($"{nameof(FastTravelBlockType)}.{_node.Type}");

    private string DefaultColour => _node.Type switch
    {
        FastTravelBlockType.Translocator => _fastTravelOverlaySettings.TranslocatorColour,
        FastTravelBlockType.Teleporter => _fastTravelOverlaySettings.TeleporterColour,
        _ => _fastTravelOverlaySettings.ErrorColour,
    };

    public static FastTravelOverlayNodeDialogue Create(FastTravelOverlayNode node) 
        => new(CrudAction.Add, node);

    public static FastTravelOverlayNodeDialogue Edit(FastTravelOverlayNode node) 
        => new(CrudAction.Edit, node);

    private FastTravelOverlayNodeDialogue(
        CrudAction mode,
        FastTravelOverlayNode node
        ) : base(ApiEx.Client)
    {
        _mode = mode;
        _node = node;
        _original = FastTravelOverlayNode.CloneFrom(node);

        _fastTravelOverlaySettings = IOC.Services.GetRequiredService<FastTravelOverlaySettings>();

        Title = T($"{_mode}.Title");
        Alignment = EnumDialogArea.CenterMiddle;
        Modal = true;
        ModalTransparency = .4f;
    }

    protected override void RefreshValues()
    {
        if (string.IsNullOrEmpty(_node.NodeColour)) _node.NodeColour = DefaultColour;
        var index = NamedColour.ValuesList().ToList().FindIndex(p => p == _node.NodeColour.IfNullOrEmpty(DefaultColour));
        SingleComposer.GetTextInput("txtTitle").SetValue(_node.Location.SourceName);
        SingleComposer.GetSwitch("btnShowPath").SetValue(_node.ShowPath);
        SingleComposer.GetDropDown("cbxColour").SetSelectedIndex(index);
        SingleComposer.GetCustomDraw("pbxColour").Redraw();

        if (_mode == CrudAction.Add) return;
        SingleComposer.GetSwitch("btnEnabled").SetValue(_node.Enabled);
    }

    protected override void ComposeBody(GuiComposer composer)
    {
        var labelFont = CairoFont.WhiteSmallText();
        var txtTitleFont = CairoFont.WhiteDetailText();
        var topBounds = ElementBounds.FixedSize(500, 30);

        //
        // Title
        //

        var left = ElementBounds.FixedSize(100, 30).FixedUnder(topBounds, 10);
        var right = ElementBounds.FixedSize(370, 30).FixedUnder(topBounds, 10).FixedRightOf(left, 10);

        composer
            .AddStaticText(T("lblTitle"), labelFont, EnumTextOrientation.Right, left, "lblTitle")
            .AddAutoSizeHoverText(T("lblTitle.HoverText"), txtTitleFont, 260, left)
            .AddTextInput(right, OnTitleChanged, txtTitleFont, "txtTitle");

        //
        // Node Colour
        //

        left = ElementBounds.FixedSize(100, 30).FixedUnder(left, 10);
        right = ElementBounds.FixedSize(370, 30).FixedUnder(right, 10).FixedRightOf(left, 10);

        var cbxColourBounds = right.FlatCopy().WithFixedWidth(330);
        var pbxColourBounds = right.FlatCopy().WithFixedWidth(30).FixedRightOf(cbxColourBounds, 10);

        var colourValues = NamedColour.ValuesList();
        var colourNames = NamedColour.NamesList();

        composer
            .AddStaticText(T("lblNodeColour"), labelFont, EnumTextOrientation.Right, left, "lblColour")
            .AddHoverText(T("lblNodeColour.HoverText"), txtTitleFont, 260, left)
            .AddDropDown(colourValues, colourNames, 0, OnColourValueChanged, cbxColourBounds, txtTitleFont, "cbxColour")
            .AddDynamicCustomDraw(pbxColourBounds, OnDrawColour, "pbxColour");

        //
        // Show Path
        //

        left = ElementBounds.FixedSize(100, 30).FixedUnder(left, 10);
        right = ElementBounds.FixedSize(370, 30).FixedUnder(right, 10).FixedRightOf(left, 10);

        composer
            .AddStaticText(T("lblShowPath"), labelFont, EnumTextOrientation.Right, left, "lblShowPath")
            .AddAutoSizeHoverText(T("lblShowPath.HoverText", BlockType), txtTitleFont, 260, left)
            .AddSwitch(OnShowPathChanged, right, "btnShowPath");

        //
        // Enabled
        //

        if (_mode == CrudAction.Edit)
        {
            left = ElementBounds.FixedSize(100, 30).FixedUnder(left, 10);
            right = ElementBounds.FixedSize(370, 30).FixedUnder(right, 10).FixedRightOf(left, 10);

            composer
                .AddStaticText(T("lblEnabled"), labelFont, EnumTextOrientation.Right, left, "lblEnabled")
                .AddAutoSizeHoverText(T("lblEnabled.HoverText"), txtTitleFont, 260, left)
                .AddSwitch(OnEnabledChanged, right, "btnEnabled");
        }

        //
        // OK Button
        //

        var buttonBounds = ElementBounds.FixedSize(100, 30).WithAlignment(EnumDialogArea.RightFixed).FixedUnder(right, 10);
        composer.AddSmallButton(LangEx.ConfirmationString("ok"), OnOkButtonPressed, buttonBounds, EnumButtonStyle.Normal, "btnSave");

        //
        // Cancel Button
        //

        buttonBounds = buttonBounds.FlatCopy().FixedLeftOf(buttonBounds, 10);
        composer.AddSmallButton(LangEx.ConfirmationString("cancel"), OnCancelButtonPressed, buttonBounds, EnumButtonStyle.Normal, "btnCancel");

        //
        // Delete Button
        //

        if (_mode == CrudAction.Add) return;
        buttonBounds = buttonBounds.FlatCopy().FixedLeftOf(buttonBounds, 10);
        composer.AddSmallButton(LangEx.ConfirmationString("delete"), OnDeleteButtonPressed, buttonBounds, EnumButtonStyle.Normal, "btnDelete");

        //
        // Reset Button
        //

        if (_mode == CrudAction.Add) return;
        buttonBounds = buttonBounds.FlatCopy().FixedLeftOf(buttonBounds, 10);
        composer.AddSmallButton(T("btnReset"), OnResetButtonPressed, buttonBounds, EnumButtonStyle.Normal, "btnReset");
    }

    private void OnColourValueChanged(string colour, bool selected)
    {
        if (!NamedColour.ValuesList().Contains(colour)) colour = DefaultColour;
        _node.NodeColour = colour;
        SingleComposer.GetCustomDraw("pbxColour").Redraw();
    }

    private void OnEnabledChanged(bool state)
    {
        _node.Enabled = state;
    }

    private void OnShowPathChanged(bool state)
    {
        _node.ShowPath = state;
    }

    private void OnTitleChanged(string title)
    {
        _node.Location.SourceName = title.IfNullOrWhitespace(T("DefaultTitle"));
    }

    private bool OnDeleteButtonPressed()
    {
        MessageBox.Show(
            title: T("DeleteConfirmationTitle"),
            message: T("DeleteConfirmationMessage", BlockType),
            buttons: ButtonLayout.OkCancel,
            onOkButtonPressed: DeleteWaypoint);
        return true;
        void DeleteWaypoint()
        {
            if (_fastTravelOverlaySettings.Nodes.RemoveAll(p => p.Location.SourcePos == _node.Location.SourcePos) > 0)
            {
                ModSettings.World.Save(_fastTravelOverlaySettings);
                ApiEx.Client.GetMapLayer<FastTravelOverlayMapLayer>()?.RebuildMapComponents();
            }
            TryClose();
        }
    }

    private bool OnOkButtonPressed()
    {
        if (_node.NodeColour == DefaultColour) _node.NodeColour = string.Empty;
        _fastTravelOverlaySettings.Nodes.RemoveAll(p => p.Location.SourcePos == _node.Location.SourcePos);
        _fastTravelOverlaySettings.Nodes.Add(_node);
        ModSettings.World.Save(_fastTravelOverlaySettings);
        ApiEx.Client.GetMapLayer<FastTravelOverlayMapLayer>()?.RebuildMapComponents();
        return TryClose();
    }

    private bool OnCancelButtonPressed()
    {
        _node.NodeColour = _original.NodeColour;
        _node.Location = _original.Location;
        _node.ShowPath = _original.ShowPath;
        _node.Enabled = _original.Enabled;
        return TryClose();
    }

    private bool OnResetButtonPressed()
    {
        _node.NodeColour = DefaultColour;
        _node.Location.SourceName = T("DefaultTitle");
        _node.ShowPath = true;
        _node.Enabled = true;
        RefreshValues();
        return true;
    }

    private void OnDrawColour(Context ctx, ImageSurface surface, ElementBounds currentBounds)
    {
        ctx.Rectangle(0.0, 0.0, GuiElement.scaled(25.0), GuiElement.scaled(25.0));
        ctx.SetSourceRGBA(ColorUtil.ToRGBADoubles(_node.NodeColour.ColourValue()));
        ctx.FillPreserve();
        ctx.SetSourceRGBA(GuiStyle.DialogBorderColor);
        ctx.Stroke();
    }

    private static FastTravelBlockType GetFastTravelBlockType(BlockEntity blockEntity)
    {
        return blockEntity switch
        {
            BlockEntityTeleporter => FastTravelBlockType.Teleporter,
            BlockEntityStaticTranslocator => FastTravelBlockType.Translocator,
            _ => FastTravelBlockType.Unknown
        };
    }

    #region Overrides

    public override EnumDialogType DialogType => EnumDialogType.Dialog;
    public override bool DisableMouseGrab => true;
    public override double DrawOrder => 0.2;
    public override bool PrefersUngrabbedMouse => true;

    #endregion
}
