using ApacheTech.VintageMods.CampaignCartographer.Features.WaypointGroups.Models;

namespace ApacheTech.VintageMods.CampaignCartographer.Features.WaypointGroups;

/// <summary>
///     Represents the settings for waypoint groups, storing a collection of defined waypoint groups.
/// </summary>
public class WaypointGroupsSettings : FeatureSettings<WaypointGroupsSettings>
{
    /// <summary>
    ///     A collection of waypoint groups configured within the world map.
    /// </summary>
    public List<WaypointGroup> Groups { get; set; } = [];
}