using ApacheTech.VintageMods.CampaignCartographer.Features.GPS.Extensions;
using Gantry.Core.GameContent.ChatCommands.Parsers;
using Gantry.Core.GameContent.ChatCommands.Parsers.Extensions;
using Gantry.Core.Hosting.Registration;
using Gantry.Services.FileSystem.Hosting;
using Gantry.Services.Network;
using Vintagestory.API.Server;

namespace ApacheTech.VintageMods.CampaignCartographer.Features.GPS.Systems;

/// <summary>
///     [GPS] The user should be able to display their current XYZ position.
///     [GPS] The user should be able to send their current XYZ position via chat, to other server members.
///     [GPS] The user should be able to copy their current XYZ position to the clipboard.
///     [GPS] The user should be able to send their current XYZ position to a specified player.
///     [GPS] The user should be able to send their current XYZ position to other players, as a clickable link that sets a waypoint on their map.
/// </summary>
/// <seealso cref="ServerModSystem" />
/// <seealso cref="IServerServiceRegistrar" />
internal class GpsServerSystem : ServerModSystem, IServerServiceRegistrar
{
    private IServerNetworkChannel _serverChannel;

    public void ConfigureServerModServices(IServiceCollection services, ICoreServerAPI sapi)
    {
        services.AddFeatureGlobalSettings<GpsSettings>();
    }

    public override void StartServerSide(ICoreServerAPI api)
    {
        G.Log("Starting GPS service.");
        var parsers = api.ChatCommands.Parsers;

        var command = api.ChatCommands
            .Create("gps")
            .RequiresPrivilege(Privilege.chat)
            .WithDescription(T("Command.Description.Default"))
            .HandleWith(ShowLocation);

        command.BeginSubCommand("chat")
            .WithDescription(T("Command.Description.BroadcastSubCommand"))
            .HandleWith(OnServerSubCommandBroadcast)
            .EndSubCommand();

        command.BeginSubCommand("copy")
            .WithDescription(T("Command.Description.ClipboardSubCommand"))
            .HandleWith(OnServerSubCommandClipboard)
            .EndSubCommand();

        command.BeginSubCommand("pm")
            .WithDescription(T("Command.Description.PrivateMessageSubCommand"))
            .WithArgs(parsers.ServerPlayers())
            .HandleWith(OnServerSubCommandPrivateMessage)
            .EndSubCommand();

        _serverChannel = IOC.Services
            .GetRequiredService<IServerNetworkService>()
            .GetOrRegisterChannel("CC_GPS")
            .RegisterMessageType<GpsPacket>();
    }

    private static TextCommandResult ShowLocation(TextCommandCallingArgs args)
    {
        var pos = args.Caller.Player.GpsLocation();
        return TextCommandResult.Success(pos);
    }

    private TextCommandResult OnServerSubCommandBroadcast(TextCommandCallingArgs args)
    {
        _serverChannel.SendPacket(new GpsPacket { Action = GpsAction.Broadcast }, args.Caller.Player as IServerPlayer);
        return TextCommandResult.Success();
    }

    private TextCommandResult OnServerSubCommandClipboard(TextCommandCallingArgs args)
    {
        _serverChannel.SendPacket(new GpsPacket { Action = GpsAction.Clipboard }, args.Caller.Player as IServerPlayer);
        return TextCommandResult.Success(T("ClipboardTextSet"));
    }

    private TextCommandResult OnServerSubCommandPrivateMessage(TextCommandCallingArgs args)
    {
        var parser = args.Parsers[0].To<GantryPlayersArgParser>();
        var searchTerm = parser.SearchTerm;
        var players = parser.GetValue().To<PlayerUidName[]>().ToList();

        switch (players.Count)
        {
            case 1:
                var target = players[0];
                SendLocationTo(args.Caller.Player, target.Uid);
                return TextCommandResult.Success();
            case > 1:
                return TextCommandResult.Error(T("FuzzyPlayerSearch.MultipleResults", searchTerm));
            default:
                return TextCommandResult.Error(T("FuzzyPlayerSearch.NoResults", searchTerm));
        }
    }

    private void SendLocationTo(IPlayer fromPlayer, string targetUid)
    {
        var toPlayer = ApiEx.ServerMain.AllOnlinePlayers.FirstOrDefault(p => p.PlayerUID == targetUid);
        var message = fromPlayer.GpsLocation();

        if (toPlayer is null)
        {
            Sapi.SendMessage(fromPlayer, GlobalConstants.GeneralChatGroup, T("player-not-found"), EnumChatType.OwnMessage);
            return;
        }

        var receivedMessage = T("whisper-received", fromPlayer.PlayerName, message);
        Sapi.SendMessage(toPlayer as IServerPlayer, GlobalConstants.GeneralChatGroup, receivedMessage, EnumChatType.OwnMessage);

        var sentMessage = T("whisper-sent", toPlayer.PlayerName, message);
        Sapi.SendMessage(fromPlayer, GlobalConstants.GeneralChatGroup, sentMessage, EnumChatType.OwnMessage);
    }

    private static string T(string path, params object[] args)
        => LangEx.FeatureString("GPS", path, args);
}