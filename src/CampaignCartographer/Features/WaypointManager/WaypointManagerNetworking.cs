using Vintagestory.API.Server;
using Gantry.Core.Annotation;
using Gantry.Services.Network.Packets;
using Gantry.Core.GameContent.GUI.Abstractions;
using System.Diagnostics;
using ApacheTech.Common.Extensions.Harmony;
using Gantry.Services.Network.Extensions;

namespace ApacheTech.VintageMods.CampaignCartographer.Features.WaypointManager;

public sealed class WaypointManagerNetworking : UniversalModSystem
{
    [ClientSide]
    public override void StartClientSide(ICoreClientAPI capi)
    {
        G.Log("Registered WaypointManager network channel.");
        capi.Network
            .GetOrRegisterDefaultChannel()
            .RegisterPacket<WorldMapTeleportPacket>()
            .RegisterPacket<WaypointActionPacket>();
    }

    [ServerSide]
    public override void StartServerSide(ICoreServerAPI api)
    {
        api.Network
            .GetOrRegisterDefaultChannel()
            .RegisterPacket<WorldMapTeleportPacket>(OnTeleportPacketReceived)
            .RegisterPacket<WaypointActionPacket>(OnWaypointPacketReceived);
        G.Log("Registered WaypointManager network channel.");
    }

    [ServerSide]
    private void OnTeleportPacketReceived(IServerPlayer fromPlayer, WorldMapTeleportPacket packet)
    {
        fromPlayer.Entity.TeleportTo(packet.Position);
    }

    [ServerSide]
    private void OnWaypointPacketReceived(IServerPlayer fromPlayer, WaypointActionPacket packet)
    {
        switch (packet.Mode)
        {
            case CrudAction.Add:
                AddWaypoint(packet.Waypoint, fromPlayer);
                break;
            case CrudAction.Edit:
                EditWaypoint(packet.Waypoint, fromPlayer);
                break;
            default:
                throw new UnreachableException();
        }
    }

    [ServerSide]
    private void AddWaypoint(Waypoint waypoint, IServerPlayer player)
    {
        var mapManager = Sapi.ModLoader.GetModSystem<WorldMapManager>();
        var waypointMapLayer = mapManager.WaypointMapLayer();
        var wpIndex = waypointMapLayer.AddWaypoint(waypoint, player);

        var message = Lang.Get("Ok, waypoint nr. {0} added", wpIndex);
        Sapi.SendMessage(player, GlobalConstants.GeneralChatGroup, message, EnumChatType.CommandSuccess);
    }

    [ServerSide]
    private void EditWaypoint(Waypoint waypoint, IServerPlayer player)
    {
        var mapManager = Sapi.ModLoader.GetModSystem<WorldMapManager>();
        var waypointMapLayer = mapManager.WaypointMapLayer();

        var result = waypointMapLayer.Waypoints
            .Select((wp, index) => new { Waypoint = wp, Index = index })
            .Where(x => x.Waypoint.OwningPlayerUid == player?.PlayerUID)
            .SingleOrDefault(x => x.Waypoint.Guid == waypoint.Guid);

        if (result is null) return;

        var wpIndex = result.Index;
        var target = result.Waypoint;

        target.Position = waypoint.Position;
        target.Title = waypoint.Title;
        target.Text = waypoint.Text;
        target.Color = waypoint.Color;
        target.Icon = waypoint.Icon;
        target.ShowInWorld = waypoint.ShowInWorld;
        target.Pinned = waypoint.Pinned;

        waypointMapLayer.CallMethod("ResendWaypoints", player);

        var message = Lang.Get("Ok, waypoint nr. {0} modified", wpIndex);
        Sapi.SendMessage(player, GlobalConstants.GeneralChatGroup, message, EnumChatType.CommandSuccess);
    }
}