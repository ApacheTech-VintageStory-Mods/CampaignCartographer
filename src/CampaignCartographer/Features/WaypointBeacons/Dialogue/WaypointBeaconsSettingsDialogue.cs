using ApacheTech.Common.Extensions.Harmony;
using Gantry.Services.FileSystem.Dialogue;

namespace ApacheTech.VintageMods.CampaignCartographer.Features.WaypointBeacons.Dialogue;

/// <summary>
///     Represents the settings dialogue for the Waypoint Beacons feature.
/// </summary>
internal class WaypointBeaconsSettingsDialogue : FeatureSettingsDialogue<WaypointBeaconsSettings>
{
    /// <summary>
    ///     Initialises a new instance of the <see cref="WaypointBeaconsSettingsDialogue"/> class.
    /// </summary>
    /// <param name="capi">The core client API instance.</param>
    /// <param name="settings">The settings to manage within the dialogue.</param>
    public WaypointBeaconsSettingsDialogue(ICoreClientAPI capi, WaypointBeaconsSettings settings)
        : base(capi, settings, "WaypointBeacons")
    {
        Movable = true;
    }

    /// <inheritdoc />
    protected override void RefreshValues()
    {
        if (!IsOpened()) return;

        // Refresh slider: Title Range
        SetSliderValue("sliderTitleRange", Settings.TitleRange);

        // Refresh slider: Icon Range
        SetSliderValue("sliderIconRange", Settings.IconRange);

        // Refresh switch: Show Waypoint Prefix
        SetSwitchValue("switchShowWaypointPrefix", Settings.ShowWaypointPrefix);

        // Refresh switch: Show Waypoint Index
        SetSwitchValue("switchShowWaypointIndex", Settings.ShowWaypointIndex);

        // Refresh switch: Show Pillars
        SetSwitchValue("switchShowPillars", Settings.ShowPillars);

        // Refresh switch: Clamp Position
        SetSwitchValue("switchClampWaypointPosition", Settings.ClampWaypointPosition);
    }

    /// <summary>
    ///     Sets the value of a slider control in the dialogue.
    /// </summary>
    /// <param name="name">The name of the slider element.</param>
    /// <param name="value">The value to set on the slider.</param>
    private void SetSliderValue(string name, int value)
    {
        var slider = SingleComposer.GetSlider(name);
        slider.SetValues(value, 10, 5000, 10);
    }

    /// <summary>
    ///     Sets the value of a switch control in the dialogue.
    /// </summary>
    /// <param name="name">The name of the switch element.</param>
    /// <param name="value">The value to set on the switch.</param>
    private void SetSwitchValue(string name, bool value)
    {
        SingleComposer.GetSwitch(name).SetValue(value);
    }

    /// <inheritdoc />
    protected override void ComposeBody(GuiComposer composer)
    {
        const int switchSize = 20;
        const int switchPadding = 20;
        const double sliderWidth = 200.0;

        var font = CairoFont.WhiteSmallText();

        // Slider: Icon Range
        var labelBounds = ElementBounds.Fixed(0, GuiStyle.TitleBarHeight + 1.0, 150, switchSize);
        var formElementBounds = ElementBounds.Fixed(160, GuiStyle.TitleBarHeight, switchSize, switchSize);
        composer.AddStaticText(T("IconRange"), font, labelBounds);
        composer.AddHoverText(T("IconRange.HoverText"), font, 260, labelBounds);
        composer.AddLazySlider(OnIconRangeChanged, formElementBounds.FlatCopy().WithFixedWidth(sliderWidth), "sliderIconRange");

        // Slider: Title Range
        labelBounds = labelBounds.BelowCopy(fixedDeltaY: switchPadding + 5);
        formElementBounds = formElementBounds.BelowCopy(fixedDeltaY: switchPadding + 5);
        composer.AddStaticText(T("TitleRange"), font, labelBounds);
        composer.AddHoverText(T("TitleRange.HoverText"), font, 260, labelBounds);
        composer.AddLazySlider(OnTitleRangeChanged, formElementBounds.FlatCopy().WithFixedWidth(sliderWidth), "sliderTitleRange");

        // Switch: Show Waypoint Prefix
        labelBounds = labelBounds.BelowCopy(fixedDeltaY: switchPadding);
        formElementBounds = formElementBounds.BelowCopy(fixedDeltaY: switchPadding);
        composer.AddStaticText(T("ShowWaypointPrefix"), font, labelBounds);
        composer.AddHoverText(T("ShowWaypointPrefix.HoverText"), font, 260, labelBounds);
        composer.AddSwitch(OnShowWaypointPrefixChanged, formElementBounds, "switchShowWaypointPrefix");

        // Switch: Show Waypoint Index
        labelBounds = labelBounds.BelowCopy(fixedDeltaY: switchPadding);
        formElementBounds = formElementBounds.BelowCopy(fixedDeltaY: switchPadding);
        composer.AddStaticText(T("ShowWaypointIndex"), font, labelBounds);
        composer.AddHoverText(T("ShowWaypointIndex.HoverText"), font, 260, labelBounds);
        composer.AddSwitch(OnShowWaypointIndexChanged, formElementBounds, "switchShowWaypointIndex");

        // Switch: Show Pillars
        labelBounds = labelBounds.BelowCopy(fixedDeltaY: switchPadding);
        formElementBounds = formElementBounds.BelowCopy(fixedDeltaY: switchPadding);
        composer.AddStaticText(T("ShowPillars"), font, labelBounds);
        composer.AddHoverText(T("ShowPillars.HoverText"), font, 260, labelBounds);
        composer.AddSwitch(OnShowPillarsChanged, formElementBounds, "switchShowPillars");

        // Switch: Clamp Waypoint Position
        labelBounds = labelBounds.BelowCopy(fixedDeltaY: switchPadding);
        formElementBounds = formElementBounds.BelowCopy(fixedDeltaY: switchPadding);
        composer.AddStaticText(T("ClampWaypointPosition"), font, labelBounds);
        composer.AddHoverText(T("ClampWaypointPosition.HoverText"), font, 260, labelBounds);
        composer.AddSwitch(OnClampWaypointPositionChanged, formElementBounds, "switchClampWaypointPosition");
    }

    /// <summary>
    ///     Updates the icon range value.
    /// </summary>
    /// <param name="newIconRange">The new icon range value.</param>
    /// <returns>True if the value was updated.</returns>
    public bool OnIconRangeChanged(int newIconRange)
    {
        Settings.IconRange = newIconRange;
        return true;
    }

    /// <summary>
    ///     Updates the title range value.
    /// </summary>
    /// <param name="newTitleRange">The new title range value.</param>
    /// <returns>True if the value was updated.</returns>
    private bool OnTitleRangeChanged(int newTitleRange)
    {
        Settings.TitleRange = newTitleRange;
        return true;
    }

    /// <summary>
    ///     Updates the setting to show waypoint prefixes.
    /// </summary>
    /// <param name="show">Whether waypoint prefixes should be displayed.</param>
    private void OnShowWaypointPrefixChanged(bool show)
    {
        Settings.ShowWaypointPrefix = show;
    }

    /// <summary>
    ///     Updates the setting to show waypoint indexes.
    /// </summary>
    /// <param name="show">Whether waypoint indexes should be displayed.</param>
    private void OnShowWaypointIndexChanged(bool show)
    {
        Settings.ShowWaypointIndex = show;
    }

    /// <summary>
    ///     Updates the setting to show waypoint pillars.
    /// </summary>
    /// <param name="show">Whether waypoint pillars should be displayed.</param>
    private void OnShowPillarsChanged(bool show)
    {
        Settings.ShowPillars = show;
    }

    /// <summary>
    ///     Updates the setting to clamp waypoint positions.
    /// </summary>
    /// <param name="show">Whether waypoint positions should be clamped.</param>
    private void OnClampWaypointPositionChanged(bool show)
    {
        Settings.ClampWaypointPosition = show;
    }
}