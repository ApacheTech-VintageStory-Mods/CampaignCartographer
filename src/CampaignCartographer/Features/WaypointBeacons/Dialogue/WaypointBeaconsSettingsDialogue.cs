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
        SetSliderValue("intTitleRange", Settings.TitleRange);

        // Refresh slider: Icon Range
        SetSliderValue("intIconRange", Settings.IconRange);

        // Refresh switch: Show Waypoint Prefix
        SetSwitchValue("btnShowWaypointPrefix", Settings.ShowWaypointPrefix);

        // Refresh switch: Show Waypoint Index
        SetSwitchValue("btnShowWaypointIndex", Settings.ShowWaypointIndex);

        // Refresh switch: Show Pillars
        SetSwitchValue("btnShowPillars", Settings.ShowPillars);

        // Refresh switch: Clamp Position
        SetSwitchValue("btnClampWaypointPosition", Settings.ClampWaypointPosition);
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
        const int leftWidth = 150;

        var labelFont = CairoFont.WhiteSmallText();
        var txtTitleFont = CairoFont.WhiteDetailText();
        var topBounds = ElementBounds.FixedSize(400, 30);


        // Slider: Icon Range
        var left = ElementBounds.FixedSize(leftWidth, 30).FixedUnder(topBounds, 10);
        var right = ElementBounds.FixedSize(270, 30).FixedUnder(topBounds, 10).FixedRightOf(left, 10);
        composer
            .AddStaticText(T("IconRange"), labelFont, EnumTextOrientation.Right, left, "lblIconRange")
            .AddAutoSizeHoverText(T("IconRange.HoverText"), txtTitleFont, 260, left)
            .AddLazySlider(OnIconRangeChanged, right, "intIconRange");

        // Slider: Title Range
        left = ElementBounds.FixedSize(leftWidth, 30).FixedUnder(left, 10);
        right = ElementBounds.FixedSize(270, 30).FixedUnder(right, 10).FixedRightOf(left, 10);
        composer
            .AddStaticText(T("TitleRange"), labelFont, EnumTextOrientation.Right, left, "lblTitleRange")
            .AddAutoSizeHoverText(T("TitleRange.HoverText"), txtTitleFont, 260, left)
            .AddLazySlider(OnTitleRangeChanged, right, "intTitleRange");

        // Switch: Show Waypoint Prefix
        left = ElementBounds.FixedSize(leftWidth, 30).FixedUnder(left, 10);
        right = ElementBounds.FixedSize(270, 30).FixedUnder(right, 10).FixedRightOf(left, 10);
        composer
            .AddStaticText(T("ShowWaypointPrefix"), labelFont, EnumTextOrientation.Right, left, "lblShowWaypointPrefix")
            .AddAutoSizeHoverText(T("ShowWaypointPrefix.HoverText"), txtTitleFont, 260, left)
            .AddSwitch(OnShowWaypointPrefixChanged, right, "btnShowWaypointPrefix");

        // Switch: Show Waypoint Index
        left = ElementBounds.FixedSize(leftWidth, 30).FixedUnder(left, 10);
        right = ElementBounds.FixedSize(270, 30).FixedUnder(right, 10).FixedRightOf(left, 10);
        composer
            .AddStaticText(T("ShowWaypointIndex"), labelFont, EnumTextOrientation.Right, left, "lblShowWaypointIndex")
            .AddAutoSizeHoverText(T("ShowWaypointIndex.HoverText"), txtTitleFont, 260, left)
            .AddSwitch(OnShowWaypointIndexChanged, right, "btnShowWaypointIndex");

        // Switch: Show Pillars
        left = ElementBounds.FixedSize(leftWidth, 30).FixedUnder(left, 10);
        right = ElementBounds.FixedSize(270, 30).FixedUnder(right, 10).FixedRightOf(left, 10);
        composer
            .AddStaticText(T("ShowPillars"), labelFont, EnumTextOrientation.Right, left, "lblShowPillars")
            .AddAutoSizeHoverText(T("ShowPillars.HoverText"), txtTitleFont, 260, left)
            .AddSwitch(OnShowWaypointPrefixChanged, right, "btnShowPillars");

        // Switch: Clamp Waypoint Position
        left = ElementBounds.FixedSize(leftWidth, 30).FixedUnder(left, 10);
        right = ElementBounds.FixedSize(270, 30).FixedUnder(right, 10).FixedRightOf(left, 10);
        composer
            .AddStaticText(T("ClampWaypointPosition"), labelFont, EnumTextOrientation.Right, left, "ClampWaypointPosition")
            .AddAutoSizeHoverText(T("ClampWaypointPosition.HoverText"), txtTitleFont, 260, left)
            .AddSwitch(OnClampWaypointPositionChanged, right, "btnClampWaypointPosition");
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