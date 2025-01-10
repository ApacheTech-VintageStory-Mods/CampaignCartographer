using Gantry.Core.Annotation;
using Gantry.Services.Network;
using Vintagestory.API.Server;

namespace ApacheTech.VintageMods.CampaignCartographer.Features.WaypointSharing;

/// <summary>
///     Provides functionality for sharing waypoints between players through the network.
/// </summary>
public class WaypointSharingService : UniversalModSystem
{
    private IUniversalNetworkService _networkService;
    private IClientNetworkChannel _clientChannel;
    private IServerNetworkChannel _serverChannel;

    /// <inheritdoc />
    public override double ExecuteOrder() => 0.12;

    /// <inheritdoc />
    [Universal]
    public override void Start(ICoreAPI api)
    {
        _networkService = IOC.Services.Resolve<IUniversalNetworkService>();
        _networkService.RegisterChannel(nameof(WaypointSharingService));
    }

    #region Server Side

    /// <inheritdoc />
    [ServerSide]
    public override void StartServerSide(ICoreServerAPI api)
    {
        ApiEx.Logger.VerboseDebug("Starting waypoint sharing service.");
        _serverChannel = _networkService.ServerChannel(nameof(WaypointSharingService));
        _serverChannel.RegisterMessageType<WaypointSharingPacket>();
        _serverChannel.SetMessageHandler<WaypointSharingPacket>(HandleServerPacket);
    }

    /// <summary>
    ///     Handles incoming waypoint sharing packets on the server side.
    /// </summary>
    /// <param name="sender">The player who sent the packet.</param>
    /// <param name="packet">The waypoint sharing packet containing the waypoint to share.</param>
    [ServerSide]
    private void HandleServerPacket(IServerPlayer sender, WaypointSharingPacket packet)
    {
        ApiEx.Logger.VerboseDebug($"Waypoint sharing packet received from {sender.PlayerName}.");

        var players = string.IsNullOrEmpty(packet.PlayerId)
            ? Sapi.World.AllOnlinePlayers.Except([sender])
            : [Sapi.World.PlayerByUid(packet.PlayerId)];

        var waypointMapLayer = Sapi.ModLoader.GetModSystem<WorldMapManager>().WaypointMapLayer();

        foreach (var player in players)
        {
            if (waypointMapLayer.Waypoints.Any(p => p.OwningPlayerUid == player.PlayerUID && p.Guid == packet.Waypoint.Guid)) continue;
            var waypoint = packet.Waypoint.With(p => p.OwningPlayerUid = player.PlayerUID);
            ApiEx.Logger.VerboseDebug($"Sharing waypoint {{{waypoint.Guid}}} with {player.PlayerName}.");
            waypointMapLayer.AddWaypoint(waypoint, player as IServerPlayer);
        }
    }

    #endregion

    #region Client Side

    /// <inheritdoc />
    [ClientSide]
    public override void StartClientSide(ICoreClientAPI api)
    {
        ApiEx.Logger.VerboseDebug("Starting waypoint sharing service.");
        _clientChannel = _networkService.ClientChannel(nameof(WaypointSharingService));
        _clientChannel.RegisterMessageType<WaypointSharingPacket>();
    }

    /// <summary>
    ///     Shares a waypoint with a specific player.
    /// </summary>
    /// <param name="waypoint">The waypoint to share.</param>
    /// <param name="playerId">The ID of the player to share the waypoint with.</param>
    [ClientSide]
    public void ShareWaypoint(Waypoint waypoint, string playerId)
    {
        ApiEx.Logger.VerboseDebug("Sending waypoint sharing packet to server.");
        _clientChannel.SendPacket(new WaypointSharingPacket
        {
            Waypoint = waypoint,
            PlayerId = playerId
        });
    }

    /// <summary>
    ///     Broadcasts a waypoint to all players.
    /// </summary>
    /// <param name="waypoint">The waypoint to broadcast.</param>
    [ClientSide]
    public void BroadcastWaypoint(Waypoint waypoint)
    {
        ApiEx.Logger.VerboseDebug("Sending waypoint sharing packet to server.");
        _clientChannel.SendPacket(new WaypointSharingPacket
        {
            Waypoint = waypoint
        });
    }

    #endregion
}