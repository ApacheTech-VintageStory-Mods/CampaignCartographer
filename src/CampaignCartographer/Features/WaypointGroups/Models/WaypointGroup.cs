namespace ApacheTech.VintageMods.CampaignCartographer.Features.WaypointGroups.Models;

/// <summary>
///     Represents a group of waypoints, identified by a unique ID and a title.
/// </summary>
public class WaypointGroup
{
    /// <summary>
    ///     A unique identifier for the waypoint group.
    /// </summary>
    public Guid Id { get; init; } = Guid.NewGuid();

    /// <summary>
    ///     The title of the waypoint group.
    /// </summary>
    public string Title { get; set; }

    /// <summary>
    ///     A collection of waypoint identifiers associated with this group.
    /// </summary>
    public List<Guid> Waypoints { get; set; } = [];
}