using ApacheTech.VintageMods.CampaignCartographer.Features.ManualWaypoints.Extensions;
using Gantry.Core.Extensions.Api;

// ReSharper disable CommentTypo
// ReSharper disable StringLiteralTypo

namespace ApacheTech.VintageMods.CampaignCartographer.Features.ManualWaypoints.Systems;

/// <summary>
///     Feature: Manual Waypoint Addition
///      • Add a waypoint to a translocator, within five blocks of the player. ***`(.wptl)`***
/// </summary>
/// <seealso cref="ClientModSystem" />
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public sealed class TranslocatorWaypoints : ClientModSystem
{
    private ICoreClientAPI _capi;

    /// <summary>
    ///     Minor convenience method to save yourself the check for/cast to ICoreClientAPI in Start()
    /// </summary>
    /// <param name="capi">The core API implemented by the client. The main interface for accessing the client. Contains all sub-components, and some miscellaneous methods.</param>
    public override void StartClientSide(ICoreClientAPI capi)
    {
        G.Log.VerboseDebug("Starting translocator waypoint service.");
        (_capi = capi).ChatCommands
            .Create("wptl")
            .WithDescription(LangEx.FeatureString("PredefinedWaypoints.TranslocatorWaypoints", "Description"))
            .HandleWith(DefaultHandler);
    }

    private TextCommandResult DefaultHandler(TextCommandCallingArgs args)
    {
        var pos = _capi.World.Player.Entity.Pos.AsBlockPos;
        var block = _capi.World.GetNearestBlock<BlockStaticTranslocator>(pos, 5f, 1f, out var blockPos);

        if (block is null)
        {
            var translocatorNotFoundMessage = LangEx.FeatureString("PredefinedWaypoints.TranslocatorWaypoints", "TranslocatorNotFound");
            _capi.ShowChatMessage(translocatorNotFoundMessage);
            return TextCommandResult.Success();
        }
        block.ProcessWaypoints(blockPos);
        return TextCommandResult.Success();
    }
}