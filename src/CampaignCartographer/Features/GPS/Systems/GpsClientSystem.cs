using ApacheTech.VintageMods.CampaignCartographer.Domain.ChatCommands.Parsers;
using ApacheTech.VintageMods.CampaignCartographer.Domain.ChatCommands.Parsers.Extensions;

namespace ApacheTech.VintageMods.CampaignCartographer.Features.GPS.Systems;

/// <summary>
///     [GPS] The user should be able to display their current XYZ position.
///     [GPS] The user should be able to send their current XYZ position via chat, to other server members.
///     [GPS] The user should be able to copy their current XYZ position to the clipboard.
///     [GPS] The user should be able to send their current XYZ position to a specified player.
///     [GPS] The user should be able to send their current XYZ position to other players, as a clickable link that sets a waypoint on their map.
/// </summary>
/// <seealso cref="ClientModSystem" />
[UsedImplicitly]
internal class GpsClientSystem : ClientModSystem
{
    public override void StartClientSide(ICoreClientAPI capi)
    {
        var parsers = capi.ChatCommands.Parsers;

        var command = capi.ChatCommands
            .Create("gps")
            .WithDescription(LangEx.FeatureString("GPS", "ClientCommand.Description.Default"))
            .HandleWith(OnClientDefaultHandler);

        command.BeginSubCommand("chat")
            .WithDescription(LangEx.FeatureString("GPS", "ClientCommand.Description.BroadcastSubCommand"))
            .HandleWith(OnClientSubCommandBroadcast)
            .EndSubCommand();

        command.BeginSubCommand("copy")
            .WithDescription(LangEx.FeatureString("GPS", "ClientCommand.Description.ClipboardSubCommand"))
            .HandleWith(OnClientSubCommandClipboard)
            .EndSubCommand();

        command.BeginSubCommand("pm")
            .WithDescription(LangEx.FeatureString("GPS", "ClientCommand.Description.PrivateMessageSubCommand"))
            .WithArgs(parsers.FuzzyPlayerSearch())
            .HandleWith(OnClientSubCommandPrivateMessage)
            .EndSubCommand();
    }

    private static TextCommandResult OnClientDefaultHandler(TextCommandCallingArgs args)
    {
        var pos = PlayerLocationMessage(args.Caller.Player);
        return TextCommandResult.Success(pos);
    }

    private TextCommandResult OnClientSubCommandBroadcast(TextCommandCallingArgs args)
    {
        var pos = PlayerLocationMessage(args.Caller.Player);
        Capi.SendChatMessage(pos);
        return TextCommandResult.Success();
    }

    private TextCommandResult OnClientSubCommandClipboard(TextCommandCallingArgs args)
    {
        Capi.Input.ClipboardText = PlayerLocationMessage(args.Caller.Player);
        return TextCommandResult.Success();
    }

    private TextCommandResult OnClientSubCommandPrivateMessage(TextCommandCallingArgs args)
    {
        var parser = args.Parsers[0].To<FuzzyPlayerParser>();
        var players = parser.Results;
        var searchTerm = parser.Value;

        switch (players.Count)
        {
            case 1:
                Capi.TriggerChatMessage($"/pm {players[0].PlayerName} {PlayerLocationMessage(args.Caller.Player)}");
                return TextCommandResult.Success();
            case > 1:
                return TextCommandResult.Error(LangEx.FeatureString("FuzzyPlayerSearch", "MultipleResults", searchTerm));
            default:
                return TextCommandResult.Error(LangEx.FeatureString("FuzzyPlayerSearch", "NoResults", searchTerm));
        }
    }

    private static string PlayerLocationMessage(IPlayer player)
    {
        var pos = player.Entity.Pos.AsBlockPos;
        var displayPos = pos.RelativeToSpawn();
        var message = $"X = {displayPos.X}, Y = {displayPos.Y}, Z = {displayPos.Z}.";
        return message;
    }
}