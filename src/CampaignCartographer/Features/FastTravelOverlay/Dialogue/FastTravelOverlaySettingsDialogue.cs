using ApacheTech.VintageMods.CampaignCartographer.Features.FastTravelOverlay.MapLayer;
using Cairo;
using Gantry.Core.GameContent.AssetEnum;
using Gantry.Services.FileSystem.Dialogue;
using Vintagestory.API.MathTools;

namespace ApacheTech.VintageMods.CampaignCartographer.Features.FastTravelOverlay.Dialogue;

public class FastTravelOverlaySettingsDialogue : FeatureSettingsDialogue<FastTravelOverlaySettings>
{
    public FastTravelOverlaySettingsDialogue(ICoreClientAPI capi, FastTravelOverlaySettings settings)
        : base(capi, settings, nameof(FastTravelOverlay))
    {
        Movable = true;
        capi.GetMapLayer<FastTravelOverlayMapLayer>();        
    }

    /// <inheritdoc />
    protected override void RefreshValues()
    {
        if (!IsOpened()) return;
        SetColourDropDownValue("TranslocatorColour", Settings.TranslocatorColour);
        SetColourDropDownValue("TeleporterColour", Settings.TeleporterColour);
        SetColourDropDownValue("ErrorColour", Settings.ErrorColour);
        SetSliderValue("intErrorOpacity", Settings.ErrorOpacity);
        SetColourDropDownValue("DisabledColour", Settings.DisabledColour);
        SetSliderValue("intDisabledOpacity", Settings.DisabledOpacity);
        SetSliderValue("intPathWidth", Settings.PathWidth, max: 3);
        SetSliderValue("intNodeSize", Settings.NodeSize, max: 5);
        ApiEx.Client.GetMapLayer<FastTravelOverlayMapLayer>().RebuildMapComponents();
    }

    private void SetSliderValue(string name, int value, int min = 1, int max = 100, int step = 1)
    {
        var slider = SingleComposer.GetSlider(name);
        slider.SetValues(value, min, max, step);
    }

    private void SetColourDropDownValue(string name, string value)
    {
        var index = NamedColour.ValuesList().ToList().FindIndex(p => p == value);
        SingleComposer.GetDropDown($"cbx{name}").SetSelectedIndex(index);
        SingleComposer.GetCustomDraw($"pbx{name}").Redraw();
    }

    protected override void ComposeBody(GuiComposer composer)
    {
        var labelFont = CairoFont.WhiteSmallText();
        var txtTitleFont = CairoFont.WhiteDetailText();
        var topBounds = ElementBounds.FixedSize(400, 30);

        var colourValues = NamedColour.ValuesList();
        var colourNames = NamedColour.NamesList();

        //
        // Translocator Colour
        //

        const int leftWidth = 200;
        var left = ElementBounds.FixedSize(leftWidth, 30).FixedUnder(topBounds, 10);
        var right = ElementBounds.FixedSize(270, 30).FixedUnder(topBounds, 10).FixedRightOf(left, 10);
        var cbxBounds = right.FlatCopy().WithFixedWidth(230);
        var pbxBounds = right.FlatCopy().WithFixedWidth(30).FixedRightOf(cbxBounds, 10);

        composer
            .AddStaticText(T("lblTranslocatorColour"), labelFont, EnumTextOrientation.Right, left, "lblTranslocatorColour")
            .AddHoverText(T("lblTranslocatorColour.HoverText"), txtTitleFont, 260, left)
            .AddDropDown(colourValues, colourNames, 0, (p, _) => OnColourChanged("TranslocatorColour", p), cbxBounds, txtTitleFont, "cbxTranslocatorColour")
            .AddDynamicCustomDraw(pbxBounds, (x, y, z) => OnDrawColour("TranslocatorColour", x, y, z), "pbxTranslocatorColour");

        //
        // Teleporter Colour
        //

        left = ElementBounds.FixedSize(leftWidth, 30).FixedUnder(left, 10);
        right = ElementBounds.FixedSize(270, 30).FixedUnder(right, 10).FixedRightOf(left, 10);
        cbxBounds = right.FlatCopy().WithFixedWidth(230);
        pbxBounds = right.FlatCopy().WithFixedWidth(30).FixedRightOf(cbxBounds, 10);

        composer
            .AddStaticText(T("lblTeleporterColour"), labelFont, EnumTextOrientation.Right, left, "lblTeleporterColour")
            .AddHoverText(T("lblTeleporterColour.HoverText"), txtTitleFont, 260, left)
            .AddDropDown(colourValues, colourNames, 0, (p, _) => OnColourChanged("TeleporterColour", p), cbxBounds, txtTitleFont, "cbxTeleporterColour")
            .AddDynamicCustomDraw(pbxBounds, (x, y, z) => OnDrawColour("TeleporterColour", x, y, z), "pbxTeleporterColour");

        //
        // Error Colour
        //

        left = ElementBounds.FixedSize(leftWidth, 30).FixedUnder(left, 10);
        right = ElementBounds.FixedSize(270, 30).FixedUnder(right, 10).FixedRightOf(left, 10);
        cbxBounds = right.FlatCopy().WithFixedWidth(230);
        pbxBounds = right.FlatCopy().WithFixedWidth(30).FixedRightOf(cbxBounds, 10);

        composer
            .AddStaticText(T("lblErrorColour"), labelFont, EnumTextOrientation.Right, left, "lblErrorColour")
            .AddHoverText(T("lblErrorColour.HoverText"), txtTitleFont, 260, left)
            .AddDropDown(colourValues, colourNames, 0, (p, _) => OnColourChanged("ErrorColour", p), cbxBounds, txtTitleFont, "cbxErrorColour")
            .AddDynamicCustomDraw(pbxBounds, (x, y, z) => OnDrawColour("ErrorColour", x, y, z), "pbxErrorColour");

        //
        // Error Opacity
        //

        left = ElementBounds.FixedSize(leftWidth, 30).FixedUnder(left, 10);
        right = ElementBounds.FixedSize(270, 30).FixedUnder(right, 10).FixedRightOf(left, 10);

        composer
            .AddStaticText(T("lblErrorOpacity"), labelFont, EnumTextOrientation.Right, left, "lblErrorOpacity")
            .AddAutoSizeHoverText(T("lblErrorOpacity.HoverText"), txtTitleFont, 260, left)
            .AddLazySlider(v => OnSliderValueChanged("ErrorOpacity", v), right, "intErrorOpacity");

        //
        // Disabled Colour
        //

        left = ElementBounds.FixedSize(leftWidth, 30).FixedUnder(left, 10);
        right = ElementBounds.FixedSize(270, 30).FixedUnder(right, 10).FixedRightOf(left, 10);
        cbxBounds = right.FlatCopy().WithFixedWidth(230);
        pbxBounds = right.FlatCopy().WithFixedWidth(30).FixedRightOf(cbxBounds, 10);

        composer
            .AddStaticText(T("lblDisabledColour"), labelFont, EnumTextOrientation.Right, left, "lblDisabledColour")
            .AddHoverText(T("lblDisabledColour.HoverText"), txtTitleFont, 260, left)
            .AddDropDown(colourValues, colourNames, 0, (p, _) => OnColourChanged("DisabledColour", p), cbxBounds, txtTitleFont, "cbxDisabledColour")
            .AddDynamicCustomDraw(pbxBounds, (x, y, z) => OnDrawColour("DisabledColour", x, y, z), "pbxDisabledColour");

        //
        // Disabled Opacity
        //

        left = ElementBounds.FixedSize(leftWidth, 30).FixedUnder(left, 10);
        right = ElementBounds.FixedSize(270, 30).FixedUnder(right, 10).FixedRightOf(left, 10);

        composer
            .AddStaticText(T("lblDisabledOpacity"), labelFont, EnumTextOrientation.Right, left, "lblDisabledOpacity")
            .AddAutoSizeHoverText(T("lblDisabledOpacity.HoverText"), txtTitleFont, 260, left)
            .AddLazySlider(v => OnSliderValueChanged("DisabledOpacity", v), right, "intDisabledOpacity");

        //
        // Path Width
        //

        left = ElementBounds.FixedSize(leftWidth, 30).FixedUnder(left, 10);
        right = ElementBounds.FixedSize(270, 30).FixedUnder(right, 10).FixedRightOf(left, 10);

        composer
            .AddStaticText(T("lblPathWidth"), labelFont, EnumTextOrientation.Right, left, "lblPathWidth")
            .AddAutoSizeHoverText(T("lblPathWidth.HoverText"), txtTitleFont, 260, left)
            .AddLazySlider(v => OnSliderValueChanged("PathWidth", v), right, "intPathWidth");

        //
        // Node Size
        //

        left = ElementBounds.FixedSize(leftWidth, 30).FixedUnder(left, 10);
        right = ElementBounds.FixedSize(270, 30).FixedUnder(right, 10).FixedRightOf(left, 10);

        composer
            .AddStaticText(T("lblNodeSize"), labelFont, EnumTextOrientation.Right, left, "lblNodeSize")
            .AddAutoSizeHoverText(T("lblNodeSize.HoverText"), txtTitleFont, 260, left)
            .AddLazySlider(v => OnSliderValueChanged("NodeSize", v), right, "intNodeSize");

        //
        // OK Button
        //

        var buttonBounds = ElementBounds.FixedSize(100, 30).WithAlignment(EnumDialogArea.RightFixed).FixedUnder(right, 10);
        composer.AddSmallButton(LangEx.ConfirmationString("ok"), OnOkButtonPressed, buttonBounds, EnumButtonStyle.Normal, "btnSave");

        //
        // Reset Button
        //

        buttonBounds = buttonBounds.FlatCopy().FixedLeftOf(buttonBounds, 10);
        composer.AddSmallButton(T("btnReset"), OnResetButtonPressed, buttonBounds, EnumButtonStyle.Normal, "btnReset");
    }

    private bool OnSliderValueChanged(string key, int value)
    {
        _ = key switch
        {
            "ErrorOpacity" => Settings.ErrorOpacity = value,
            "DisabledOpacity" => Settings.DisabledOpacity = value,
            "PathWidth" => Settings.PathWidth = value,
            "NodeSize" => Settings.NodeSize = value,
            _ => 0
        };
        RefreshValues();
        return true;
    }

    private void OnColourChanged(string key, string value)
    {
        _ = key switch
        {
            "TranslocatorColour" => Settings.TranslocatorColour = value,
            "TeleporterColour" => Settings.TeleporterColour = value,
            "ErrorColour" => Settings.ErrorColour = value,
            "DisabledColour" => Settings.DisabledColour = value,
            _ => ""
        };
        RefreshValues();
    }

    private void OnDrawColour(string key, Context ctx, ImageSurface surface, ElementBounds currentBounds)
    {
        var setting = key switch
        {
            "TranslocatorColour" => Settings.TranslocatorColour.ColourValue(),
            "TeleporterColour" => Settings.TeleporterColour.ColourValue(),
            "ErrorColour" => Settings.ErrorColour.ColourValue(),
            "DisabledColour" => Settings.DisabledColour.ColourValue(),
            _ => 0
        };

        ctx.Rectangle(0.0, 0.0, GuiElement.scaled(25.0), GuiElement.scaled(25.0));
        ctx.SetSourceRGBA(ColorUtil.ToRGBADoubles(setting));
        ctx.FillPreserve();
        ctx.SetSourceRGBA(GuiStyle.DialogBorderColor);
        ctx.Stroke();
    }

    private bool OnOkButtonPressed()
    {
        return TryClose();
    }

    private bool OnResetButtonPressed()
    {
        var reset = new FastTravelOverlaySettings();
        OnColourChanged("TranslocatorColour", reset.TranslocatorColour);
        OnColourChanged("TeleporterColour", reset.TeleporterColour);
        OnColourChanged("ErrorColour", reset.ErrorColour);
        OnColourChanged("DisabledColour", reset.DisabledColour);
        OnSliderValueChanged("ErrorOpacity", reset.ErrorOpacity);
        OnSliderValueChanged("DisabledOpacity", reset.DisabledOpacity);
        OnSliderValueChanged("PathWidth", reset.PathWidth);
        OnSliderValueChanged("NodeSize", reset.NodeSize);
        RefreshValues();
        return true;
    }

    #region Overrides

    public override EnumDialogType DialogType => EnumDialogType.Dialog;
    public override bool DisableMouseGrab => true;
    public override double DrawOrder => 0.2;
    public override bool PrefersUngrabbedMouse => true;

    #endregion
}