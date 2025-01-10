using ApacheTech.VintageMods.CampaignCartographer.Features.WaypointManager.Extensions;

namespace ApacheTech.VintageMods.CampaignCartographer.Features.WaypointManager.WaypointTemplates;

/// <summary>
///     Represents a waypoint with a set position that can be added to the map within the game, with a selection state.
/// </summary>
[JsonObject]
[ProtoContract]
public class SelectableWaypointTemplate : PositionedWaypointTemplate
{
    /// <summary>
    ///     Gets or sets a value indicating whether the waypoint is selected.
    /// </summary>
    [JsonRequired]
    [ProtoMember(8)]
    public bool Selected { get; set; } = true;

    /// <summary>
    ///     Creates a new <see cref="SelectableWaypointTemplate"/> from a <see cref="Waypoint"/>.
    /// </summary>
    /// <param name="waypoint">The waypoint to create the template from.</param>
    /// <param name="selected">Optional value indicating whether the waypoint should be selected. Defaults to <c>false</c>.</param>
    /// <returns>A new instance of <see cref="SelectableWaypointTemplate"/>.</returns>
    public static SelectableWaypointTemplate FromWaypoint(Waypoint waypoint, bool selected = false)
    {
        return new SelectableWaypointTemplate
        {
            Id = waypoint.Guid,
            Colour = waypoint.Color.ToHexString(),
            DisplayedIcon = waypoint.Icon,
            ServerIcon = waypoint.Icon,
            Title = waypoint.Title,
            Pinned = waypoint.Pinned,
            Position = waypoint.Position,
            Selected = selected
        };
    }
}