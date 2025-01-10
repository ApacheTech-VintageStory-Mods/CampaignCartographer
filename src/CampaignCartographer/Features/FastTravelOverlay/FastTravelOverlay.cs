using ApacheTech.VintageMods.CampaignCartographer.Features.FastTravelOverlay.Behaviour;
using ApacheTech.VintageMods.CampaignCartographer.Features.FastTravelOverlay.Dialogue;
using ApacheTech.VintageMods.CampaignCartographer.Features.FastTravelOverlay.MapLayer;
using ApacheTech.VintageMods.CampaignCartographer.Features.ModMenu.Extensions;
using Gantry.Core.Hosting.Registration;
using Gantry.Services.FileSystem.Configuration;
using Gantry.Services.FileSystem.Hosting;

namespace ApacheTech.VintageMods.CampaignCartographer.Features.FastTravelOverlay;

/// <summary>
///     Represents the client-side logic for the Fast Travel Overlay mod, managing settings and map layers.
/// </summary>
public class FastTravelOverlay : ClientModSystem, IClientServiceRegistrar
{
    private FastTravelOverlaySettings _settings;

    /// <summary>
    ///     Configures client-side services required by the fast travel overlay feature.
    /// </summary>
    /// <param name="services">The service collection to register dependencies into.</param>
    /// <param name="capi">The core client API instance.</param>
    public void ConfigureClientModServices(IServiceCollection services, ICoreClientAPI capi)
    {
        services.AddFeatureWorldSettings<FastTravelOverlaySettings>();
        services.AddSingleton<FastTravelOverlaySettingsDialogue>();
    }

    /// <summary>
    ///     Initialises the client-side logic for the Fast Travel Overlay, including map layer and dialogue registration.
    /// </summary>
    /// <param name="api">The core client API instance.</param>
    public override void StartClientSide(ICoreClientAPI api)
    {
        ApiEx.Logger.VerboseDebug("Starting fast travel map overlay service.");
        _settings = IOC.Services.GetRequiredService<FastTravelOverlaySettings>();
        var mapManager = api.ModLoader.GetModSystem<WorldMapManager>();
        mapManager.RegisterMapLayer<FastTravelOverlayMapLayer>(nameof(FastTravelOverlay), 1.1);
        api.RegisterBlockBehaviour<FastTravelBlockBehaviour>("FastTravelOverlay");
        api.AddModMenuDialogue<FastTravelOverlaySettingsDialogue>("FastTravelOverlay");
    }

    /// <summary>
    ///     Finalises the assets for the mod, adding the FastTravelBlockBehaviour to specific blocks.
    /// </summary>
    /// <param name="api">The core client API instance.</param>
    public override void AssetsFinalise(ICoreClientAPI api)
    {
        foreach (var block in api.World.Blocks)
        {
            if (block is not (BlockStaticTranslocator or BlockTeleporter)) continue;
            ApiEx.Logger.VerboseDebug($"Adding FastTravelBlockBehaviour to: {block.Code}");
            var behaviour = new FastTravelBlockBehaviour(block, _settings);
            block.BlockBehaviors = [.. block.BlockBehaviors, behaviour];
        }
    }

    /// <summary>
    ///     Updates the teleporter target information based on the provided location.
    /// </summary>
    /// <param name="location">The teleporter location with source and target information.</param>
    public void UpdateTeleporterTarget(TeleporterLocation location)
    {
        var node = _settings.Nodes.FirstOrDefault(p => p.Location.SourcePos == location.SourcePos);
        if (node is null) return;
        node.Location.TargetName = location.TargetName;
        node.Location.TargetPos = location.TargetPos;
        ModSettings.World.Save(_settings);
        ApiEx.Client.GetMapLayer<FastTravelOverlayMapLayer>().RebuildMapComponents();
    }

    /// <summary>
    ///     Translates a string resource.
    /// </summary>
    /// <param name="path">The path of the string resource.</param>
    /// <param name="args">Arguments for string formatting.</param>
    /// <returns>The translated string.</returns>
    public static string T(string path, params object[] args)
        => LangEx.FeatureString($"{nameof(FastTravelOverlay)}.Dialogue", path, args);
}