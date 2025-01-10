namespace ApacheTech.VintageMods.CampaignCartographer.Features.WaypointManager.WaypointTemplates;

/// <summary>
///     Represents the minimal properties of a waypoint.
/// </summary>
public interface IMinimalWaypoint
{
    /// <summary>
    ///     Gets or sets the globally unique identifier (GUID) of the waypoint.
    /// </summary>
    string Guid { get; set; }

    /// <summary>
    ///     Gets or sets the title of the waypoint.
    /// </summary>
    string Title { get; set; }

    /// <summary>
    ///     Gets or sets the colour of the waypoint, represented as an integer value.
    /// </summary>
    int Color { get; set; }

    /// <summary>
    ///     Gets or sets the icon of the waypoint.
    /// </summary>
    string Icon { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating whether the waypoint is pinned.
    /// </summary>
    bool Pinned { get; set; }
}