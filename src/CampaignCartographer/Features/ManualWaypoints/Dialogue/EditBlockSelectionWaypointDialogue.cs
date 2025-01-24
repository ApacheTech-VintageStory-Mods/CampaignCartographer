using ApacheTech.Common.Extensions.Harmony;
using ApacheTech.VintageMods.CampaignCartographer.Features.WaypointManager.WaypointTemplates;
using Gantry.Core.GameContent.GUI.Abstractions;
using Gantry.Services.FileSystem.Configuration;
using Gantry.Services.FileSystem.Dialogue;
using Vintagestory.API.MathTools;
using Vintagestory.API.Util;

namespace ApacheTech.VintageMods.CampaignCartographer.Features.ManualWaypoints.Dialogue;

/// <summary>
///     GUI window that enables the player to be able to edit the template for the Block Selection Waypoints they add.
/// </summary>
/// <seealso cref="GenericDialogue" />
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public class EditBlockSelectionWaypointDialogue : FeatureSettingsDialogue<PredefinedWaypointsSettings>
{
    private readonly CoverageWaypointTemplate _waypoint;
    private readonly PredefinedWaypointsSettings _settings;

    private readonly string[] _icons;
    private readonly int[] _colours;

    /// <summary>
    /// 	Initialises a new instance of the <see cref="EditBlockSelectionWaypointDialogue"/> class.
    /// </summary>
    /// <param name="settings">The current settings that contain the template to edit.</param>
    public EditBlockSelectionWaypointDialogue(PredefinedWaypointsSettings settings) : base(ApiEx.Client, settings)
    {
        Title = LangEx.FeatureString("PredefinedWaypoints.Dialogue.BlockSelection", "Title");
        ModalTransparency = 0.4f;
        Alignment = EnumDialogArea.CenterMiddle;
        Modal = true;

        var waypointMapLayer = IOC.Services.GetRequiredService<WaypointMapLayer>();
        _icons = [.. waypointMapLayer.WaypointIcons.Keys];
        _colours = [.. waypointMapLayer.WaypointColors];
        _settings = settings;
        _waypoint = _settings.BlockSelectionWaypointTemplate.Clone<CoverageWaypointTemplate>();
    }

    #region Form Composition

    protected override void RefreshValues()
    {
        var colour = _colours.IndexOf(_waypoint.Colour.ColourValue());
        var icon = _icons.IndexOf(_waypoint.DisplayedIcon);
        SingleComposer.ColorListPickerSetValue("optColour", Math.Max(colour, 0));
        SingleComposer.IconListPickerSetValue("optIcon", Math.Max(icon, 0));
        SingleComposer.GetSlider("txtHorizontalRadius").SetValues(_waypoint.HorizontalCoverageRadius, 0, 50, 1);
        SingleComposer.GetSlider("txtVerticalRadius").SetValues(_waypoint.VerticalCoverageRadius, 0, 50, 1);
    }

    protected override void ComposeBody(GuiComposer composer)
    {
        var labelFont = CairoFont.WhiteSmallText();
        var txtTitleFont = CairoFont.WhiteDetailText();
        var topBounds = ElementBounds.FixedSize(600, 30);

        //
        // Horizontal Radius
        //

        var left = ElementBounds.FixedSize(100, 30).FixedUnder(topBounds, 10);
        var right = ElementBounds.FixedSize(470, 30).FixedUnder(topBounds, 10).FixedRightOf(left, 10);

        composer
            .AddStaticText(LangEx.FeatureString("PredefinedWaypoints.Dialogue.BlockSelection", "HCoverage"), labelFont, EnumTextOrientation.Right, left, "lblHorizontalRadius")
            .AddHoverText(LangEx.FeatureString("PredefinedWaypoints.Dialogue.BlockSelection", "HCoverage.HoverText"), txtTitleFont, 260, left)
            .AddLazySlider(OnHorizontalRadiusChanged, right.FlatCopy().WithFixedHeight(20), "txtHorizontalRadius");

        //
        // Vertical Radius
        //

        left = ElementBounds.FixedSize(100, 30).FixedUnder(left, 10);
        right = ElementBounds.FixedSize(470, 30).FixedUnder(right, 10).FixedRightOf(left, 10);

        composer
            .AddStaticText(LangEx.FeatureString("PredefinedWaypoints.Dialogue.BlockSelection", "VCoverage"), labelFont, EnumTextOrientation.Right, left, "lblVerticalRadius")
            .AddHoverText(LangEx.FeatureString("PredefinedWaypoints.Dialogue.BlockSelection", "VCoverage.HoverText"), txtTitleFont, 260, left)
            .AddLazySlider(OnVerticalRadiusChanged, right.FlatCopy().WithFixedHeight(20), "txtVerticalRadius");

        //
        // Colour
        //

        const double colourIconSize = 22d;
        left = ElementBounds.FixedSize(100, 30).FixedUnder(left, 10);
        right = ElementBounds.FixedSize(270, 30).FixedUnder(right, 10).FixedRightOf(left, 10);

        composer
            .AddStaticText(T("BlockSelection.Colour"), labelFont, EnumTextOrientation.Right, left, "lblColour")
            .AddAutoSizeHoverText(T("BlockSelection.Colour.HoverText"), txtTitleFont, 260, left)
            .AddColorListPicker(_colours, OnColourSelected, right.WithFixedSize(colourIconSize, colourIconSize), 470, "optColour");

        //
        // Icon
        //

        const double iconIconSize = 27d;
        left = ElementBounds.FixedSize(100, 30).FixedUnder(right, 10);
        right = ElementBounds.FixedSize(270, 30).FixedUnder(right, 10).FixedRightOf(left, 10);

        composer
            .AddStaticText(T("BlockSelection.Icon"), labelFont, EnumTextOrientation.Right, left, "lblIcon")
            .AddAutoSizeHoverText(T("BlockSelection.Icon.HoverText"), txtTitleFont, 260, left)
            .AddIconListPicker(_icons, OnIconSelected, right.WithFixedSize(iconIconSize, iconIconSize), 470, "optIcon");

        //
        // OK Button
        //

        left = ElementBounds.FixedSize(100, 30).FixedUnder(right);
        var buttonBounds = ElementBounds.FixedSize(100, 30).WithAlignment(EnumDialogArea.RightFixed).FixedUnder(left, 10);
        composer.AddSmallButton(LangEx.ConfirmationString("ok"), OnOkButtonPressed, buttonBounds, EnumButtonStyle.Normal, "btnSave");

        //
        // Cancel Button
        //

        buttonBounds = buttonBounds.FlatCopy().FixedLeftOf(buttonBounds, 10);
        composer.AddSmallButton(LangEx.ConfirmationString("cancel"), OnCancelButtonPressed, buttonBounds, EnumButtonStyle.Normal, "btnCancel");
    }

    #endregion

    #region Control Event Handlers

    private void OnIconSelected(int index)
    {
        _waypoint.DisplayedIcon = _icons[index];
        _waypoint.ServerIcon = _icons[index];
    }

    private void OnColourSelected(int index)
    {
        _waypoint.Colour = ColorUtil.Int2Hex(_colours[index]);
    }

    private bool OnHorizontalRadiusChanged(int radius)
    {
        _waypoint.HorizontalCoverageRadius = radius;
        return true;
    }

    private bool OnVerticalRadiusChanged(int radius)
    {
        _waypoint.VerticalCoverageRadius = radius;
        return true;
    }

    private bool OnCancelButtonPressed()
    {
        return TryClose();
    }

    private bool OnOkButtonPressed()
    {
        _settings.BlockSelectionWaypointTemplate = _waypoint;
        ModSettings.World.Save(_settings);
        return TryClose();
    }

    #endregion
}