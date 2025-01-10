namespace ApacheTech.VintageMods.CampaignCartographer.Features.WaypointBeacons;

/// <summary>
///     Represents the settings for the Waypoint Beacons feature.
/// </summary>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
[ProtoContract(ImplicitFields = ImplicitFields.AllFields)]
public class WaypointBeaconsSettings : FeatureSettings<WaypointBeaconsSettings>
{
    /// <summary>
    ///     The range, in blocks, within which waypoint icons are visible.
    /// </summary>
    public int IconRange { get; set; } = 2000;

    /// <summary>
    ///     Indicates whether waypoint prefixes should be displayed.
    /// </summary>
    public bool ShowWaypointPrefix { get; set; } = true;

    /// <summary>
    ///     Indicates whether waypoint indexes should be displayed.
    /// </summary>
    public bool ShowWaypointIndex { get; set; } = true;

    /// <summary>
    ///     Indicates whether waypoint pillars should be displayed.
    /// </summary>
    public bool ShowPillars { get; set; } = true;

    /// <summary>
    ///     Whether to clamp the waypoint to the side of the screen.
    /// </summary>
    public bool ClampWaypointPosition { get; set; }

    /// <summary>
    ///     The range, in blocks, within which waypoint titles are visible.
    /// </summary>
    public int TitleRange { get; set; } = 500;

    /// <summary>
    ///     The list of waypoints that have been selected to float.
    /// </summary>
    public List<string> ActiveBeacons { get; set; } = [];
}