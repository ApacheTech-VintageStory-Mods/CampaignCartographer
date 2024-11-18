using ProtoBuf;
using Vintagestory.API.MathTools;

namespace ApacheTech.VintageMods.CampaignCartographer.Features.WaypointManager.WaypointTemplates;

/// <summary>
///     Represents a Waypoint, with a set position, that can be added the the map within the game.
/// </summary>
[JsonObject]
[ProtoContract]
public class PositionedWaypointTemplate : WaypointTemplate
{
    [JsonRequired]
    [ProtoMember(7)]
    public Vec3d Position { get; set; } = Vec3d.Zero;
}