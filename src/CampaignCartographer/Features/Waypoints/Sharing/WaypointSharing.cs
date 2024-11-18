using Gantry.Core.Annotation;
using Gantry.Services.Network;
using Vintagestory.API.Server;

namespace ApacheTech.VintageMods.CampaignCartographer.Features.Waypoints.Sharing;

public class WaypointSharing : UniversalModSystem
{
    private IUniversalNetworkService _networkService;
    private IClientNetworkChannel _clientChannel;
    private IServerNetworkChannel _serverChannel;
    private string _lastMessage;

    [Universal]
    public override void Start(ICoreAPI api)
    {
        _networkService = IOC.Services.Resolve<IUniversalNetworkService>();
        _networkService.RegisterChannel(nameof(WaypointSharing));
    }

    [ServerSide]
    public override void StartServerSide(ICoreServerAPI api)
    {
        _serverChannel = _networkService.ServerChannel(nameof(WaypointSharing));
        _serverChannel.RegisterMessageType<WaypointSharingPacket>();
        _serverChannel.SetMessageHandler<WaypointSharingPacket>(new(HandleServerPacket));
    }

    [ServerSide]
    public void SendWaypointToClient(string message)
    {
        if (string.IsNullOrEmpty(message)) return;
        _serverChannel.SendPacket(WaypointSharingPacket.WithMessage(message));
    }

    [ServerSide]
    private void HandleServerPacket(IServerPlayer player, WaypointSharingPacket packet)
    {
        _serverChannel.BroadcastPacket(packet, [player]);
    }

    [ClientSide]
    public override void StartClientSide(ICoreClientAPI api)
    {
        _clientChannel = _networkService.ClientChannel(nameof(WaypointSharing));
        _clientChannel.RegisterMessageType<WaypointSharingPacket>();
        _clientChannel.SetMessageHandler<WaypointSharingPacket>(new(HandleClientPacket));
    }

    [ClientSide]
    public void SendWaypointToServer(string message)
    {
        if (string.IsNullOrEmpty(message)) return;
        _clientChannel.SendPacket(WaypointSharingPacket.WithMessage(message));
    }

    [ClientSide]
    private void HandleClientPacket(WaypointSharingPacket packet)
    {
        if (_lastMessage == packet.Message) return;
        Capi.TriggerChatMessage(packet.Message);
        _lastMessage = packet.Message;
    }
}