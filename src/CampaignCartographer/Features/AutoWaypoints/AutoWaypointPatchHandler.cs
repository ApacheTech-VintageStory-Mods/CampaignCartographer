using ApacheTech.VintageMods.CampaignCartographer.Features.WaypointManager.Extensions;
using ApacheTech.VintageMods.CampaignCartographer.Features.WaypointManager.WaypointTemplates;
using Gantry.Core.Hosting.Annotation;
using Gantry.Services.FileSystem.Abstractions.Contracts;
using Gantry.Services.FileSystem.Configuration.Consumers;

namespace ApacheTech.VintageMods.CampaignCartographer.Features.AutoWaypoints;

/// <summary>
///     Acts as a mediator between block patches, and the waypoint service.
/// </summary>
/// <seealso cref="WorldSettingsConsumer{T}" />
[UsedImplicitly]
public sealed class AutoWaypointPatchHandler : WorldSettingsConsumer<AutoWaypointsSettings>
{
    private readonly WaypointTemplateService _service;
    private readonly CrossMaps _crossMaps;

    /// <summary>
    ///     Initialises a new instance of the <see cref="AutoWaypointPatchHandler"/> class.
    /// </summary>
    /// <param name="waypointService">The waypoint service.</param>
    /// <param name="fileSystemService">The file system service.</param>
    [SidedConstructor(EnumAppSide.Client)]
    public AutoWaypointPatchHandler(WaypointTemplateService waypointService, IFileSystemService fileSystemService)
    {
        _service = waypointService;
        _crossMaps = fileSystemService.GetJsonFile("crossmap.json").ParseAs<CrossMaps>();
    }

    /// <summary>
    ///     Called when a player interacts with a block.
    ///     Filters out uninteresting blocks, and passes interesting blocks to the waypoint service for processing.
    /// </summary>
    /// <param name="block">The block that was interacted with.</param>
    public void HandleInteraction(Block block)
    {
        switch (block)
        {
            case BlockLooseOres:
                HandleMinerals(block);
                break;
            case BlockLooseStones:
                HandleLooseStones(block);
                break;
            case BlockMushroom:
                HandleMushrooms(block);
                break;
            case BlockBerryBush:
                HandleBerries(block);
                break;
            default:
                // For some reason, the resin log is not of type BlockLog.
                if (!block.Variant.TryGetValue("type", out var value)) break;
                if (value.StartsWith("resin"))
                    HandleResin(block);
                break;
        }
    }

    /// <summary>
    ///     Handles mineral blocks, forwarding them to either ore or stone processing.
    /// </summary>
    /// <param name="block">The mineral block.</param>
    private void HandleMinerals(Block block)
    {
        if (!HandleLooseOres(block)) HandleLooseStones(block);
    }

    /// <summary>
    ///     Handles mushroom blocks and adds them to the waypoint service if enabled.
    /// </summary>
    /// <param name="block">The mushroom block.</param>
    private void HandleMushrooms(Block block)
    {
        if (!Settings.Mushrooms) return;
        AddWaypointFor(block, MapOrganicMaterial(block.Code.Path));
    }

    /// <summary>
    ///     Handles berry bush blocks and adds them to the waypoint service if enabled.
    /// </summary>
    /// <param name="block">The berry bush block.</param>
    private void HandleBerries(Block block)
    {
        if (!Settings.BerryBushes) return;
        AddWaypointFor(block, MapOrganicMaterial(block.Code.Path));
    }

    /// <summary>
    ///     Handles resin blocks and adds them to the waypoint service if enabled.
    /// </summary>
    /// <param name="block">The resin block.</param>
    private void HandleResin(Block block)
    {
        if (!Settings.Resin) return;
        AddWaypointFor(block, MapOrganicMaterial(block.Code.Path));
    }

    /// <summary>
    ///     Handles loose stone blocks and adds them to the waypoint service if enabled.
    /// </summary>
    /// <param name="block">The loose stone block.</param>
    private void HandleLooseStones(Block block)
    {
        if (!Settings.LooseStones) return;
        AddWaypointFor(block, MapLooseStones(block.Code.Path));
    }

    /// <summary>
    ///     Handles loose ore blocks and adds them to the waypoint service if enabled.
    /// </summary>
    /// <param name="block">The loose ore block.</param>
    /// <returns><see langword="true"/> if the ore was added, otherwise <see langword="false"/>.</returns>
    private bool HandleLooseOres(Block block)
    {
        return Settings.SurfaceDeposits &&
        AddWaypointFor(block, MapSurfaceOre(block.Code.Path));
    }

    private string MapSurfaceOre(string assetCode) => Map(_crossMaps.Ores, assetCode);
    private string MapLooseStones(string assetCode) => Map(_crossMaps.Stones, assetCode);
    private string MapOrganicMaterial(string assetCode) => Map(_crossMaps.Organics, assetCode);

    private static string Map(IDictionary<string, string> dictionary, string assetCode)
    {
        var value = dictionary.FirstOrNull(p => assetCode.Contains(p.Key));
        if (!string.IsNullOrWhiteSpace(value)) return value;
        var exception = new ArgumentOutOfRangeException(nameof(assetCode), assetCode, $"Could not map {assetCode} to any waypoint template.");
        ApiEx.Logger.Error(exception);
        return null;
    }

    /// <summary>
    ///     Adds a waypoint for the specified block at the player's position.
    /// </summary>
    /// <param name="block">The block for which to add a waypoint.</param>
    /// <param name="syntax">The syntax defining the waypoint type.</param>
    /// <returns><see langword="true"/> if the waypoint was added, otherwise <see langword="false"/>.</returns>
    private bool AddWaypointFor(Block block, string syntax)
    {
        if (string.IsNullOrWhiteSpace(syntax)) return false;
        var position = ApiEx.ClientMain.Player.Entity.BlockSelection.Position;
        var title = block.GetPlacedBlockName(ApiEx.ClientMain, position);
        var waypoint = _service.GetTemplateByKey(syntax);

        if (waypoint is null)
        {
            ApiEx.Client.EnqueueShowChatMessage(LangEx.FeatureString("PredefinedWaypoints", "InvalidSyntax", syntax));
            return false;
        }

        waypoint
            .With(p => p.Title = title)
            .AddToMap(position);
        return true;
    }
}