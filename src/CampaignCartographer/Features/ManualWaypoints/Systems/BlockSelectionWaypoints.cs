using ApacheTech.VintageMods.CampaignCartographer.Features.WaypointManager.Extensions;
using ApacheTech.VintageMods.CampaignCartographer.Features.WaypointManager.WaypointTemplates;
using Gantry.Services.FileSystem.Configuration;

namespace ApacheTech.VintageMods.CampaignCartographer.Features.ManualWaypoints.Systems;

/// <summary>
///     Feature: Manual Waypoint Addition
///      • Add a waypoint for the block the player is currently targetting. `(.wps)`
/// </summary>
/// <seealso cref="ClientModSystem" />
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public sealed class BlockSelectionWaypoints : ClientModSystem
{
    private ICoreClientAPI _capi;

    /// <summary>
    ///     Minor convenience method to save yourself the check for/cast to ICoreClientAPI in Start()
    /// </summary>
    /// <param name="capi">
    ///     The core API implemented by the client.
    ///     The main interface for accessing the client.
    ///     Contains all subcomponents, and some miscellaneous methods.
    /// </param>
    public override void StartClientSide(ICoreClientAPI capi)
    {
        G.Log("Starting the block selection waypoint service.");
        (_capi = capi).ChatCommands
            .Create("wps")
            .WithDescription(LangEx.FeatureString("PredefinedWaypoints.BlockSelectionWaypoints", "Description"))
            .HandleWith(DefaultHandler);
    }

    private TextCommandResult DefaultHandler(TextCommandCallingArgs args)
    {
        var blockSelection = _capi.World.Player.CurrentBlockSelection;
        if (blockSelection is null) return TextCommandResult.Success();
        var position = blockSelection.Position;
        var block = _capi.World.BlockAccessor.GetBlock(position, BlockLayersAccess.Default);
        var title = block.GetPlacedBlockName(_capi.World, position);

        var template = ModSettings.World.
            Feature<PredefinedWaypointsSettings>()
            .BlockSelectionWaypointTemplate;

        var waypoint = new PredefinedWaypointTemplate
        {
            Colour = template.Colour,
            DisplayedIcon = template.DisplayedIcon,
            ServerIcon = template.ServerIcon,
            Title = title,
            HorizontalCoverageRadius = template.HorizontalCoverageRadius,
            VerticalCoverageRadius = template.VerticalCoverageRadius
        };
        waypoint.AddToMap(position);
        return TextCommandResult.Success();
    }
}