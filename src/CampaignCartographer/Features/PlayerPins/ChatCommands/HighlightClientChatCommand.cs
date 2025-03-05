using System.Text;
using Gantry.Core.GameContent.ChatCommands.Parsers;
using Gantry.Core.GameContent.ChatCommands.Parsers.Extensions;
using Gantry.Services.FileSystem.Configuration;

namespace ApacheTech.VintageMods.CampaignCartographer.Features.PlayerPins.ChatCommands;

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public class HighlightClientChatCommand
{
    private readonly PlayerPinsSettings _settings;
    private readonly ICoreClientAPI _capi;

    public HighlightClientChatCommand(ICoreClientAPI capi, PlayerPinsSettings settings)
    {
        _settings = settings;
        _capi = capi;
    }

    public void Register()
    {
        var playerPins = _capi.ChatCommands.Get("playerpins");
        var parsers = _capi.ChatCommands.Parsers;

        var highlight = playerPins
            .BeginSubCommand("highlight")
            .WithDescription(LangEx.FeatureString("PlayerPins", "Highlight.Description"))
            .HandleWith(DefaultHandler);

        highlight
            .BeginSubCommand("list")
            .WithDescription(LangEx.FeatureString("PlayerPins", "Highlight.List.Description"))
            .HandleWith(DefaultHandler)
            .EndSubCommand();

        highlight
            .BeginSubCommand("add")
            .WithDescription(LangEx.FeatureString("PlayerPins", "Highlight.Add.Description"))
            .WithArgs(parsers.ServerPlayers())
            .HandleWith(OnAdd)
            .EndSubCommand();

        highlight
            .BeginSubCommand("remove")
            .WithDescription(LangEx.FeatureString("PlayerPins", "Highlight.Remove.Description"))
            .WithArgs(parsers.ServerPlayers())
            .HandleWith(OnRemove)
            .EndSubCommand();

        highlight.EndSubCommand();
    }

    private TextCommandResult OnAdd(TextCommandCallingArgs args)
    {
        var parser = args.Parsers[0].To<GantryPlayersArgParser>();
        var searchTerm = parser.SearchTerm;
        var players = parser.GetValue().To<PlayerUidName[]>().ToList();

        switch (players.Count)
        {
            case 1:
                var player = players.First();
                if (_settings.HighlightedPlayers.ContainsValue(player.Uid))
                {
                    return UserFeedback("PlayerAlreadyAdded", player.Name);
                }
                _settings.HighlightedPlayers.Add(player.Name, player.Uid);
                ModSettings.World.Save(_settings);
                return UserFeedback("PlayerAdded", player.Name);
            case > 1:
                return TextCommandResult.Error(LangEx.FeatureString("FuzzyPlayerSearch", "MultipleResults", searchTerm));
            default:
                return TextCommandResult.Error(LangEx.FeatureString("FuzzyPlayerSearch", "NoResults", searchTerm));
        }
    }

    private TextCommandResult OnRemove(TextCommandCallingArgs args)
    {
        var parser = args.Parsers[0].To<GantryPlayersArgParser>();
        var searchTerm = parser.SearchTerm;
        var players = parser.GetValue().To<PlayerUidName[]>().ToList();

        switch (players.Count)
        {
            case 1:
                var player = players.First();
                if (!_settings.HighlightedPlayers.ContainsValue(player.Uid))
                {
                    return UserFeedback("PlayerNotOnList", player.Name);
                }
                _settings.HighlightedPlayers.Remove(player.Name);
                ModSettings.World.Save(_settings);
                return UserFeedback("PlayerRemoved", player.Name);
            case > 1:
                return TextCommandResult.Error(LangEx.FeatureString("FuzzyPlayerSearch", "MultipleResults", searchTerm));
            default:
                return TextCommandResult.Error(LangEx.FeatureString("FuzzyPlayerSearch", "NoResults", searchTerm));
        }
    }

    private TextCommandResult DefaultHandler(TextCommandCallingArgs args)
    {
        if (_settings.HighlightedPlayers.Count == 0)
        {
            return UserFeedback("NoPlayersHighlighted");
        }

        var sb = new StringBuilder(LangEx.FeatureString("PlayerPins", "Highlight.PlayersList"));
        sb.AppendLine("\n");

        foreach (var player in _settings.HighlightedPlayers.Keys)
        {
            sb.AppendLine(player);
        }

        return TextCommandResult.Success(sb.ToString());
    }

    private static TextCommandResult UserFeedback(string action, params object[] args)
        => TextCommandResult.Success(LangEx.FeatureString("PlayerPins", $"Highlight.{action}", args));
}