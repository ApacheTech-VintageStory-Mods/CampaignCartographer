namespace ApacheTech.VintageMods.CampaignCartographer.Features.WaypointManager.WaypointTemplates;

/// <summary>
///     Represents a Waypoint that the user can add to the map by used a specific key.
/// </summary>
[JsonObject]
[ProtoContract]
public class PredefinedWaypointTemplate : CoverageWaypointTemplate
{
    /// <summary>
    ///     The syntax the user must type to add this type of waypoint.
    /// </summary>
    /// <value>The syntax value of the waypoint.</value>
    [JsonRequired]
    [ProtoMember(9)]
    public string Key { get; set; } = string.Empty;

    /// <summary>
    ///     Determines whether this waypoint type is enabled for manual waypoint addition.
    /// </summary>
    /// <returns><c>true</c> if this waypoint type should be added to the manual waypoints syntax list; otherwise <c>false</c></returns>
    [ProtoMember(10)]
    public bool Enabled { get; set; } = true;
}