using ApacheTech.VintageMods.CampaignCartographer.Features.ManualWaypoints.Model;
using ApacheTech.VintageMods.CampaignCartographer.Features.WaypointManager.Extensions;
using ApacheTech.VintageMods.CampaignCartographer.Features.WaypointManager.WaypointTemplates;
using Gantry.Core.GameContent.AssetEnum;

// ReSharper disable StringLiteralTypo

namespace ApacheTech.VintageMods.CampaignCartographer.Features.ManualWaypoints.Systems;

/// <summary>
///     Feature: Manual Waypoint Addition
///      • Add a waypoint to a trader, within five blocks of the player. (.wpt)
/// </summary>
/// <seealso cref="ClientModSystem" />
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public sealed class TraderWaypoints : ClientModSystem
{
    private ICoreClientAPI _capi;
    private WaypointTemplateService _waypointService;

    /// <inheritdoc />
    public override void StartClientSide(ICoreClientAPI capi)
    {
        G.Log("Starting the trader waypoint service.");
        _waypointService = IOC.Services.Resolve<WaypointTemplateService>();

        (_capi = capi).ChatCommands
            .Create("wpt")
            .WithDescription(LangEx.FeatureString("PredefinedWaypoints.TraderWaypoints", "Description"))
            .HandleWith(DefaultHandler);
    }

    private TextCommandResult DefaultHandler(TextCommandCallingArgs args)
    {
        var found = false;

        var trader = (EntityTrader)_capi.World.GetNearestEntity(_capi.World.Player.Entity.Pos.XYZ, 10f, 10f, p =>
        {
            if (!p.Code.Path.StartsWith("humanoid-trader-") || !p.Alive) return false;
            found = true;
            return true;
        });

        if (!found)
        {
            _capi.ShowChatMessage(LangEx.FeatureString("PredefinedWaypoints.TraderWaypoints", "TraderNotFound"));
        }
        else
        {
            var displayName = trader.GetBehavior<EntityBehaviorNameTag>().DisplayName;
            var wpTitle = Lang.Get("tradingwindow-" + trader.Code.Path, displayName);

            new PredefinedWaypointTemplate
            {
                Title = wpTitle,
                Colour = TraderModel.GetColourFor(trader),
                DisplayedIcon = WaypointIcon.Trader,
                ServerIcon = WaypointIcon.Trader
            }.AddToMap(trader.Pos.AsBlockPos);

        }
        return TextCommandResult.Success();
    }
}