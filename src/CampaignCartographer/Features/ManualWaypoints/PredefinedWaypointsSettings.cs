using ApacheTech.VintageMods.CampaignCartographer.Features.WaypointManager.WaypointTemplates;

namespace ApacheTech.VintageMods.CampaignCartographer.Features.ManualWaypoints;

/// <summary>
///     Represents a DTO object that stores settings information for the Manual Waypoints feature.
/// </summary>
[JsonObject]
public sealed class PredefinedWaypointsSettings : FeatureSettings<PredefinedWaypointsSettings>
{
    /// <summary>
    ///     Gets or sets the block selection waypoint template.
    /// </summary>
    /// <value>The block selection waypoint template.</value>
    public CoverageWaypointTemplate BlockSelectionWaypointTemplate { get; set; } = new();

    /// <summary>
    ///     The template packs that have been disabled.
    /// </summary>
    public List<string> DisabledTemplatePacks { get; set; } = [];

    /// <summary>
    ///     The templates within a pack that have been disabled.
    /// </summary>
    public Dictionary<string, List<string>> DisabledTemplatesPerPack { get; set; } = [];
}