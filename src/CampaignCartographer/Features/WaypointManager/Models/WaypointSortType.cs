namespace ApacheTech.VintageMods.CampaignCartographer.Features.WaypointManager.Models;

/// <summary>
///     Specifies the different sorting types for waypoints.
/// </summary>
public enum WaypointSortType
{
    /// <summary>
    ///     Sorts waypoints by index in ascending order.
    /// </summary>
    IndexAscending,

    /// <summary>
    ///     Sorts waypoints by index in descending order.
    /// </summary>
    IndexDescending,

    /// <summary>
    ///     Sorts waypoints by colour in ascending order.
    /// </summary>
    ColourAscending,

    /// <summary>
    ///     Sorts waypoints by colour in descending order.
    /// </summary>
    ColourDescending,

    /// <summary>
    ///     Sorts waypoints by name in ascending order.
    /// </summary>
    NameAscending,

    /// <summary>
    ///     Sorts waypoints by name in descending order.
    /// </summary>
    NameDescending,

    /// <summary>
    ///     Sorts waypoints by distance from the player in ascending order.
    /// </summary>
    DistanceAscending,

    /// <summary>
    ///     Sorts waypoints by distance from the player in descending order.
    /// </summary>
    DistanceDescending
}