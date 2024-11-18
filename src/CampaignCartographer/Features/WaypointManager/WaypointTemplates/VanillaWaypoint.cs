using ProtoBuf;
using Vintagestory.API.MathTools;

namespace ApacheTech.VintageMods.CampaignCartographer.Features.WaypointManager.WaypointTemplates;

[ProtoContract]
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public class VanillaWaypoint
{
    [JsonConstructor]
    public VanillaWaypoint() { }

    /// <summary>
    ///  	Initialises a new instance of the <see cref="VanillaWaypoint"/> class.
    /// </summary>
    /// <param name="waypoint">The waypoint to decorate.</param>
    public VanillaWaypoint(Waypoint waypoint)
    {
        Guid = waypoint.Guid;
        Position = waypoint.Position;
        Title = waypoint.Title;
        Text = waypoint.Text;
        Color = waypoint.Color;
        Icon = waypoint.Icon;
        ShowInWorld = waypoint.ShowInWorld;
        Pinned = waypoint.Pinned;
        OwningPlayerUid = waypoint.OwningPlayerUid;
        OwningPlayerGroupId = waypoint.OwningPlayerGroupId;
        Temporary = waypoint.Temporary;
    }

    [ProtoMember(11)]
    public string Guid { get; set; }

    [ProtoMember(6)]
    public Vec3d Position { get; set; }

    [ProtoMember(10)]
    public string Title { get; set; }

    [ProtoMember(9)]
    public string Text { get; set; }

    [ProtoMember(1)]
    public int Color { get; set; }

    [ProtoMember(2)]
    public string Icon { get; set; }

    [ProtoMember(7)]
    public bool ShowInWorld { get; set; }

    [ProtoMember(5)]
    public bool Pinned { get; set; }

    [ProtoMember(4)]
    public string OwningPlayerUid { get; set; }

    [ProtoMember(3)]
    public int OwningPlayerGroupId { get; set; }

    [ProtoMember(8)]
    public bool Temporary { get; set; }

    public static implicit operator VanillaWaypoint(Waypoint waypoint) => new(waypoint);

    public static implicit operator Waypoint(VanillaWaypoint waypoint) => new()
    {
        Guid = waypoint.Guid,
        Position = waypoint.Position,
        Title = waypoint.Title,
        Text = waypoint.Text,
        Color = waypoint.Color,
        Icon = waypoint.Icon,
        ShowInWorld = waypoint.ShowInWorld,
        Pinned = waypoint.Pinned,
        OwningPlayerUid = waypoint.OwningPlayerUid,
        OwningPlayerGroupId = waypoint.OwningPlayerGroupId,
        Temporary = waypoint.Temporary
    };
}