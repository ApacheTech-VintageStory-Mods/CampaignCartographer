using ApacheTech.VintageMods.CampaignCartographer.Features.ManualWaypoints.Model;
using ApacheTech.VintageMods.CampaignCartographer.Features.WaypointManager.Extensions;
using ApacheTech.VintageMods.CampaignCartographer.Features.WaypointManager.WaypointTemplates;
using Gantry.Core.GameContent.AssetEnum;
using Vintagestory.GameContent;

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
    private ICoreClientAPI? _capi;

    /// <inheritdoc />
    public override void StartClientSide(ICoreClientAPI capi)
    {
        G.Log("Starting the trader waypoint service.");
        (_capi = capi).ChatCommands
            .Create("wpt")
            .WithDescription(LangEx.FeatureString("PredefinedWaypoints.TraderWaypoints", "Description"))
            .HandleWith(DefaultHandler);
    }

    private TextCommandResult DefaultHandler(TextCommandCallingArgs args)
    {
        var found = false;
        if (_capi?.World is null)
        {
            G.Logger.Error("The world is not available, cannot add trader waypoint.");
            return TextCommandResult.Error("Campaign Cartographer encountered an error: CC-1001");
        }

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
            var title = Lang.Get("tradingwindow-" + trader.Code.Path, displayName);

            var waypointService = IOC.Services.GetRequiredService<WaypointTemplateService>();
            var template = waypointService.GetTemplateByKey("trader");

            var waypoint = new PredefinedWaypointTemplate
            {
                Title = title,
                Colour = TraderModel.GetColourFor(trader),
                DisplayedIcon = template?.DisplayedIcon ?? WaypointIcon.Trader!,
                ServerIcon = template?.ServerIcon ?? WaypointIcon.Trader!,
                VerticalCoverageRadius = template?.VerticalCoverageRadius ?? 25,
                HorizontalCoverageRadius = template?.HorizontalCoverageRadius ?? 25,
                Pinned = template?.Pinned ?? false
            };
            
            waypoint.AddToMap(trader.Pos.AsBlockPos);

        }
        return TextCommandResult.Success();
    }
}