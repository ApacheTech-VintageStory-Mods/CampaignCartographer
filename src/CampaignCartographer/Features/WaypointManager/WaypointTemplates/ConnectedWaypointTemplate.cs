namespace ApacheTech.VintageMods.CampaignCartographer.Features.WaypointManager.WaypointTemplates;

/// <summary>
///     Represents a Waypoint that has a connection to another waypoint on the map.
/// </summary>
[JsonObject]
[ProtoContract]
public class ConnectedWaypointTemplate : WaypointTemplate
{
    /// <summary>
    ///     The ID of the Waypoint that this Waypoint is connected to.
    /// </summary>
    /// <value>A <see cref="string"/> representing the Waypoint that is connected to this one.</value>
    [ProtoMember(7)]
    public string EndPoint { get; set; }
}