using ApacheTech.VintageMods.CampaignCartographer.Features.ModMenu.Extensions;
using ApacheTech.VintageMods.CampaignCartographer.Features.WaypointBeacons.Dialogue;
using ApacheTech.VintageMods.CampaignCartographer.Features.WaypointBeacons.Dialogue.Renderers;
using Gantry.Core.Extensions.Threading;
using Gantry.Core.GameContent;
using Gantry.Core.Hosting.Registration;
using Gantry.Services.FileSystem.Hosting;

namespace ApacheTech.VintageMods.CampaignCartographer.Features.WaypointBeacons;

/// <summary>
///     Represents the client-side mod system for managing waypoint beacons.
/// </summary>
public class WaypointBeacons : ClientModSystem, IClientServiceRegistrar
{
    private WaypointBeaconsClientSystem _clientSystem;

    /// <summary>
    ///     Configures client-side services required by the waypoint beacons mod.
    /// </summary>
    /// <param name="services">The service collection to register dependencies into.</param>
    /// <param name="capi">The core client API instance.</param>
    public void ConfigureClientModServices(IServiceCollection services, ICoreClientAPI capi)
    {
        services.AddFeatureWorldSettings<WaypointBeaconsSettings>();
        services.AddSingleton<WaypointBeaconsSettingsDialogue>();
    }

    /// <inheritdoc />
    public override double ExecuteOrder() => 0.22;

    /// <inheritdoc />
    public override void StartClientSide(ICoreClientAPI capi)
    {
        G.Log.VerboseDebug("Starting waypoint beacons client mod system.");
        capi.Event.LevelFinalize += OnLevelFinalise;
        capi.AddModMenuDialogue<WaypointBeaconsSettingsDialogue>("WaypointBeacons");
    }

    /// <summary>
    ///     Handles the event when the level has been finalised, preparing waypoint beacons for rendering.
    /// </summary>
    private void OnLevelFinalise()
    {
        G.Log.VerboseDebug("Caching all waypoint icons for beacons");
        Capi.Event.LevelFinalize += () => WaypointIconFactory.PreCacheAllIcons(Capi);

        G.Log.VerboseDebug("Creating Waypoint Beacons render mesh flyweight");
        WaypointBeaconStore.Create();

        G.Log.VerboseDebug("Injecting Waypoint Beacons client system");
        _clientSystem = new WaypointBeaconsClientSystem(ApiEx.ClientMain);
        Capi.InjectClientThread("WaypointBeacons", _clientSystem);
    }

    /// <inheritdoc cref="ModSystem.Dispose" />
    public override void Dispose()
    {
        Capi.Event.LevelFinalize -= OnLevelFinalise;
        WaypointIconFactory.Dispose();
    }
}