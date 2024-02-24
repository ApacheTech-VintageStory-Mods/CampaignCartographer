using ProtoBuf;
using Vintagestory.API.MathTools;

namespace ApacheTech.VintageMods.CampaignCartographer.Features.WaypointManager.Abstractions;

public interface IWaypointSpecificRequest
{
    public Guid WaypointId { get; set; }
}

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

public class Waypoints
{
    private readonly WorldMapManager _worldMapManager;
    private readonly WaypointMapLayer _waypointMapLayer;
    private readonly ICoreClientAPI _capi;
    private readonly List<Waypoint> _ownWaypoints;

    public Waypoints(
        WorldMapManager worldMapManager,
        WaypointMapLayer waypointMapLayer,
        ICoreClientAPI capi
        )
    {
        _worldMapManager = worldMapManager ?? throw new ArgumentNullException(nameof(worldMapManager));
        _waypointMapLayer = waypointMapLayer;
        _capi = capi;
        _ownWaypoints = _waypointMapLayer.ownWaypoints;

        _capi.SendChatMessage("/waypoint");
    }

    public int FindIndex(Waypoint waypoint) 
        => _ownWaypoints.FindIndex(w => w.Guid == waypoint.Guid);


    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class NativeWaypointCommands
    {
        public static void Add(BlockPos position, string icon, bool pinned, string colour, string title) =>
            ApiEx.Client.SendChatMessage($"/waypoint addati {icon} {position.X} {position.Y} {position.Z} {pinned} {colour} {title}");

        public static void Modify(int index, string icon, bool pinned, string colour, string title) =>
            ApiEx.Client.SendChatMessage($"/waypoint modify {index} {colour} {icon} {pinned} {title}");

        public static void Remove(int index) =>
            ApiEx.Client.SendChatMessage($"/waypoint remove {index}");
    }
}

