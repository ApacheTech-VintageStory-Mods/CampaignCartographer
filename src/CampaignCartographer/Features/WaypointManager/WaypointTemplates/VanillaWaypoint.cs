using Vintagestory.API.MathTools;

namespace ApacheTech.VintageMods.CampaignCartographer.Features.WaypointManager.WaypointTemplates;

/// <summary>
///     Represents a vanilla waypoint, used for decorating and serializing waypoint data.
/// </summary>
[ProtoContract]
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public class VanillaWaypoint
{
    /// <summary>
    ///     Initialises a new instance of the <see cref="VanillaWaypoint"/> class from an existing <see cref="Waypoint"/> object.
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

    /// <summary>
    ///     Gets or sets the unique identifier of the waypoint.
    /// </summary>
    [ProtoMember(11)]
    public string Guid { get; set; }

    /// <summary>
    ///     Gets or sets the position of the waypoint in the world.
    /// </summary>
    [ProtoMember(6)]
    public Vec3d Position { get; set; }

    /// <summary>
    ///     Gets or sets the title of the waypoint.
    /// </summary>
    [ProtoMember(10)]
    public string Title { get; set; }

    /// <summary>
    ///     Gets or sets the text description of the waypoint.
    /// </summary>
    [ProtoMember(9)]
    public string Text { get; set; }

    /// <summary>
    ///     Gets or sets the colour of the waypoint.
    /// </summary>
    [ProtoMember(1)]
    public int Color { get; set; }

    /// <summary>
    ///     Gets or sets the icon associated with the waypoint.
    /// </summary>
    [ProtoMember(2)]
    public string Icon { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating whether the waypoint should be shown in the world.
    /// </summary>
    [ProtoMember(7)]
    public bool ShowInWorld { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating whether the waypoint is pinned.
    /// </summary>
    [ProtoMember(5)]
    public bool Pinned { get; set; }

    /// <summary>
    ///     Gets or sets the unique identifier of the player who owns the waypoint.
    /// </summary>
    [ProtoMember(4)]
    public string OwningPlayerUid { get; set; }

    /// <summary>
    ///     Gets or sets the group ID of the player who owns the waypoint.
    /// </summary>
    [ProtoMember(3)]
    public int OwningPlayerGroupId { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating whether the waypoint is temporary.
    /// </summary>
    [ProtoMember(8)]
    public bool Temporary { get; set; }

    /// <summary>
    ///     Implicitly converts a <see cref="Waypoint"/> to a <see cref="VanillaWaypoint"/>.
    /// </summary>
    /// <param name="waypoint">The waypoint to convert.</param>
    public static implicit operator VanillaWaypoint(Waypoint waypoint) => new(waypoint);

    /// <summary>
    ///     Implicitly converts a <see cref="VanillaWaypoint"/> to a <see cref="Waypoint"/>.
    /// </summary>
    /// <param name="waypoint">The vanilla waypoint to convert.</param>
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