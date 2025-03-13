using Vintagestory.API.MathTools;

namespace ApacheTech.VintageMods.CampaignCartographer.Features.WaypointManager.WaypointTemplates;

/// <summary>
///     Represents a waypoint with a set position that can be added to the map within the game.
/// </summary>
[JsonObject]
[ProtoContract]
public class PositionedWaypointTemplate : WaypointTemplateBase
{
    /// <summary>
    ///     Gets or sets the position of the waypoint in the game world.
    /// </summary>
    [JsonRequired]
    [ProtoMember(7)]
    public Vec3d Position { get; set; } = Vec3d.Zero;
}