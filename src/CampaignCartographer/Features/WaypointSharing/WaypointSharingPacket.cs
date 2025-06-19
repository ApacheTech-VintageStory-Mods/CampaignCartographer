namespace ApacheTech.VintageMods.CampaignCartographer.Features.WaypointSharing;

/// <summary>
///     Represents a data packet for sharing waypoint information between clients or with the server.
/// </summary>
[ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
public class WaypointSharingPacket
{
    /// <summary>
    ///     The waypoint being shared within this packet.
    /// </summary>
    public Waypoint Waypoint { get; set; } = default!;

    /// <summary>
    ///     The UID of the player to share the waypoint with.
    ///     If the string is null or empty, the waypoint will be shared with everyone online.
    /// </summary>
    public string PlayerId { get; set; } = default!;
}