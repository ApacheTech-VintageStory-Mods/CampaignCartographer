using ApacheTech.VintageMods.CampaignCartographer.Features.WaypointManager.Extensions;
using ProtoBuf;

namespace ApacheTech.VintageMods.CampaignCartographer.Features.WaypointManager.WaypointTemplates;

/// <summary>
///     Represents a Waypoint, with a set position, that can be added the map within the game.
/// </summary>
[JsonObject]
[ProtoContract]
public class SelectableWaypointTemplate : PositionedWaypointTemplate
{
    [JsonRequired]
    [ProtoMember(8)]
    public bool Selected { get; set; } = true;

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