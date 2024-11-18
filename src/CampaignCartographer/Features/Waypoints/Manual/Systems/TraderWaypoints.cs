using ApacheTech.VintageMods.CampaignCartographer.Features.WaypointManager.Extensions;
using ApacheTech.VintageMods.CampaignCartographer.Features.WaypointManager.WaypointTemplates;
using ApacheTech.VintageMods.CampaignCartographer.Features.Waypoints.Manual.Model;

// ReSharper disable StringLiteralTypo

namespace ApacheTech.VintageMods.CampaignCartographer.Features.Waypoints.Manual.Systems;

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

    /// <summary>
    ///     Minor convenience method to save yourself the check for/cast to ICoreClientAPI in Start()
    /// </summary>
    /// <param name="capi">The core API implemented by the client. The main interface for accessing the client. Contains all sub-components, and some miscellaneous methods.</param>
    public override void StartClientSide(ICoreClientAPI capi)
    {
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

            _waypointService.GetTemplateByKey("trader")?
                .With(p =>
                {
                    p.Title = wpTitle;
                    p.Colour = TraderModel.GetColourFor(trader);
                })
                .AddToMap(trader.Pos.AsBlockPos);
        }
        return TextCommandResult.Success();
    }
}