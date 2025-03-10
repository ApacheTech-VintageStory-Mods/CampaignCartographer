﻿namespace ApacheTech.VintageMods.CampaignCartographer.Features.WaypointManager.WaypointTemplates;

/// <summary>
///     Represents a Waypoint that cannot be placed within a specific radius of another waypoint of the same type.
/// </summary>
[JsonObject]
[ProtoContract]
public class CoverageWaypointTemplate : WaypointTemplateBase
{
    /// <summary>
    ///     Determines how far apart waypoints of the same type can be.
    /// </summary>
    /// <value>The minimum distance between two waypoints of the same type.</value>
    [ProtoMember(7)]
    public int HorizontalCoverageRadius { get; set; } = 10;

    /// <summary>
    ///     Determines how far apart waypoints of the same type can be.
    /// </summary>
    /// <value>The minimum distance between two waypoints of the same type.</value>
    [ProtoMember(8)]
    public int VerticalCoverageRadius { get; set; } = 10;
}