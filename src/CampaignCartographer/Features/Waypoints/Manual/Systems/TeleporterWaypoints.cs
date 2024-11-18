using ApacheTech.VintageMods.CampaignCartographer.Domain.Extensions;
using Gantry.Core.Extensions.Api;

// ReSharper disable StringLiteralTypo

namespace ApacheTech.VintageMods.CampaignCartographer.Features.Waypoints.Manual.Systems;

/// <summary>
///     Feature: Manual Waypoint Addition
///      • Add a waypoint to a teleporter block, within five blocks of the player.(.wptp)
/// </summary>
/// <seealso cref="ClientModSystem" />
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public sealed class TeleporterWaypoints : ClientModSystem
{
    private ICoreClientAPI _capi;

    /// <summary>
    ///     Minor convenience method to save yourself the check for/cast to ICoreClientAPI in Start()
    /// </summary>
    /// <param name="capi">The core API implemented by the client. The main interface for accessing the client. Contains all sub-components, and some miscellaneous methods.</param>
    public override void StartClientSide(ICoreClientAPI capi)
    {
        (_capi = capi).ChatCommands
            .Create("wptp")
            .WithDescription(LangEx.FeatureString("PredefinedWaypoints.TeleporterWaypoints", "Description"))
            .HandleWith(DefaultHandler);
    }

    private TextCommandResult DefaultHandler(TextCommandCallingArgs args)
    {
        var found = false;
        var pos = _capi.World.Player.Entity.Pos.AsBlockPos;
        var teleporter = _capi.World.GetNearestBlockEntity<BlockEntityTeleporter>(pos, 5f, 1f, Predicate);

        if (!found)
        {
            var teleporterNotFoundMessage = LangEx.FeatureString("PredefinedWaypoints.TeleporterWaypoints", "TeleporterNotFound");
            _capi.ShowChatMessage(teleporterNotFoundMessage);
            return TextCommandResult.Success();
        }

        var titleTemplate = LangEx.FeatureCode("PredefinedWaypoints.TeleporterWaypoints", "TeleporterWaypointTitle");
        teleporter.AddWaypoint(titleTemplate);
        return TextCommandResult.Success();

        bool Predicate(BlockEntityTeleporter p) => found = true;
    }
}