using System.Threading.Tasks;
using ApacheTech.Common.Extensions.Harmony;
using ApacheTech.VintageMods.CampaignCartographer.Features.WaypointManager.Extensions;
using ApacheTech.VintageMods.CampaignCartographer.Features.WaypointManager.Repositories;
using ApacheTech.VintageMods.CampaignCartographer.Features.WaypointManager.WaypointTemplates;
using Gantry.Core.GameContent.AssetEnum;
using Vintagestory.API.MathTools;

namespace ApacheTech.VintageMods.CampaignCartographer.Features.ManualWaypoints.Extensions;

/// <summary>
///     Provides extension methods for <see cref="BlockEntityStaticTranslocator"/> and related functionality.
/// </summary>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public static class BlockEntityStaticTranslocatorExtensions
{
    /// <summary>
    ///     Adds waypoints at each end of a fixed translocator.
    /// </summary>
    /// <param name="translocator">The translocator to add the waypoints to.</param>
    /// <param name="titleTemplate">The title of the waypoint. Can include format placeholders for X, Y, and Z.</param>
    public static void AddWaypointsForEndpoints(this BlockEntityStaticTranslocator translocator, string titleTemplate)
    {
        Task.Factory.StartNew(async () =>
        {
            var blockPos = translocator.Pos;
            var targetPos = translocator.TargetLocation;
            await AddWaypointAsync(blockPos, targetPos, titleTemplate);
            await AddWaypointAsync(targetPos, blockPos, titleTemplate);
        });
    }

    /// <summary>
    ///     Removes waypoints associated with a specific block position.
    /// </summary>
    /// <param name="pos">The position of the block whose waypoints are to be cleared.</param>
    /// <returns>A task that resolves to true if waypoints were removed, false otherwise.</returns>
    private static async Task<bool> ClearWaypointsForEndpoint(BlockPos pos)
    {
        var service = IOC.Services.Resolve<WaypointTemplateService>();
        var repo = IOC.Services.Resolve<WaypointCommandsRepository>();
        var model = service.GetTemplateByKey("tl");

        var retVal = await pos.WaypointExistsAtPosAsync(Filter);
        if (retVal) repo.RemoveAllWaypointsAtPosition(pos);

        return retVal;

        bool Filter(Waypoint p)
        {
            var iconMatches = string.Equals(p.Icon, model.ServerIcon, StringComparison.InvariantCultureIgnoreCase);
            var colourMatches = p.Color == NamedColour.Red.ToArgb();
            return iconMatches && colourMatches;
        }
    }

    /// <summary>
    ///     Adds a waypoint for the given source and destination positions.
    /// </summary>
    /// <param name="sourcePos">The source position for the waypoint.</param>
    /// <param name="destPos">The destination position for the waypoint.</param>
    /// <param name="titleTemplate">The title template for the waypoint, supporting positional placeholders.</param>
    private static async Task AddWaypointAsync(BlockPos sourcePos, BlockPos destPos, string titleTemplate)
    {
        var displayPos = destPos.RelativeToSpawn();
        var message = Lang.Get(titleTemplate, displayPos.X, displayPos.Y, displayPos.Z);
        var forceWaypoint = await ClearWaypointsForEndpoint(sourcePos);

        new PredefinedWaypointTemplate
        {
            Title = message,
            Colour = NamedColour.Fuchsia,
            DisplayedIcon = WaypointIcon.Spiral,
            ServerIcon = WaypointIcon.Spiral
        }.AddToMap(sourcePos, forceWaypoint);

        G.Logger.VerboseDebug($"Added Waypoint: Translocator to ({displayPos.X}, {displayPos.Y}, {displayPos.Z})");
    }

    /// <summary>
    ///     Adds a waypoint for a broken translocator.
    /// </summary>
    /// <param name="block">The translocator block that is broken.</param>
    /// <param name="blockPos">The position of the broken translocator block.</param>
    public static void AddBrokenTranslocatorWaypoint(this BlockStaticTranslocator block, BlockPos blockPos)
    {
        var message = LangEx.FeatureString("PredefinedWaypoints.TranslocatorWaypoints", "BrokenTranslocatorTitle");
        var displayPos = blockPos.RelativeToSpawn();
        if (blockPos.WaypointExistsAtPos(p => p.Icon == WaypointIcon.Spiral)) return;

        new PredefinedWaypointTemplate
        {
            Title = message,
            Colour = NamedColour.Red,
            DisplayedIcon = WaypointIcon.Spiral,
            ServerIcon = WaypointIcon.Spiral
        }.AddToMap(blockPos);

        G.Logger.VerboseDebug($"Added Waypoint: Broken Translocator at ({displayPos.X}, {displayPos.Y}, {displayPos.Z})");
    }

    /// <summary>
    ///     Processes waypoints for a given translocator block.
    /// </summary>
    /// <param name="block">The translocator block being processed.</param>
    /// <param name="blockPos">The position of the translocator block.</param>
    public static void ProcessWaypoints(this BlockStaticTranslocator block, BlockPos blockPos)
    {
        var translocator = (BlockEntityStaticTranslocator)ApiEx.Client.World.GetBlockAccessorPrefetch(false, false).GetBlockEntity(blockPos);
        if (!block.Repaired || translocator is null || !translocator.GetField<bool>("canTeleport"))
        {
            block.AddBrokenTranslocatorWaypoint(blockPos);
            return;
        }
        if (!translocator.FullyRepaired) return;
        var titleTemplate = LangEx.FeatureCode("PredefinedWaypoints.TranslocatorWaypoints", "TranslocatorWaypointTitle");
        translocator.AddWaypointsForEndpoints(titleTemplate);
    }
}