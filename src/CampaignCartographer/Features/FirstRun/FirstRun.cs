using Gantry.Core.Hosting.Registration;
using Gantry.Services.FileSystem.Abstractions.Contracts;
using Gantry.Services.FileSystem.Hosting;
using Gantry.Services.FileSystem.Enums;
using ApacheTech.VintageMods.CampaignCartographer.Features.WaypointManager.WaypointTemplates;
using ApacheTech.VintageMods.CampaignCartographer.Features.WaypointManager.Repositories;
using ApacheTech.VintageMods.CampaignCartographer.Features.FirstRun.Dialogue;

namespace ApacheTech.VintageMods.CampaignCartographer.Features.FirstRun;

/// <summary>
///     Registers types for the FirstRun feature with the IOC container.
/// </summary>
/// <seealso cref="IClientServiceRegistrar" />
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public sealed class FirstRun : ClientModSystem, IClientServiceRegistrar
{
    private FirstRunWorldSettings _worldSettings;
    private FirstRunGlobalSettings _globalSettings;
    private IFileSystemService _fileSystemService;
    private List<PredefinedWaypointTemplate> _defaultWaypoints;
    private long _listenerId;

    /// <summary>
    ///     Allows a mod to include Singleton, or Transient services to the IOC Container.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="capi">The client-side API.</param>
    public void ConfigureClientModServices(IServiceCollection services, ICoreClientAPI capi)
    {
        services.AddTransient<FirstRunDialogue>();
        services.AddFeatureGlobalSettings<FirstRunGlobalSettings>();
        services.AddFeatureWorldSettings<FirstRunWorldSettings>();
    }

    protected override void StartPreClientSide(ICoreClientAPI capi)
    {
        _fileSystemService = IOC.Services.Resolve<IFileSystemService>()
            .RegisterFile("default-waypoints.json", FileScope.Global)
            .RegisterFile("version.data", FileScope.Global)
            .RegisterFile("waypoint-types.json", FileScope.Global);

        _defaultWaypoints = _fileSystemService
            .GetJsonFile("default-waypoints.json")
            .ParseAsMany<PredefinedWaypointTemplate>()
            .ToList();

        _fileSystemService
            .GetJsonFile("waypoint-types.json")
            .SaveFrom(_defaultWaypoints);
    }

    public override void StartClientSide(ICoreClientAPI api)
    {
        _globalSettings = IOC.Services.Resolve<FirstRunGlobalSettings>();
        _worldSettings = IOC.Services.Resolve<FirstRunWorldSettings>();

        if (_worldSettings.FirstRun) HandleFirstRun();
    }

    private void HandleFirstRun()
    {
        if (!_globalSettings.NeverLoadDefaultWaypoints)
        {
            _listenerId = Capi.Event.RegisterGameTickListener(OnGameTick, 100);
        }
        return;

        void OnGameTick(float dt)
        {
            var defaultWaypointsFile = _fileSystemService.GetJsonFile("default-waypoints.json");
            if (!defaultWaypointsFile.AsFileInfo().Exists)
            {
                return;
            }

            Capi.Event.UnregisterGameTickListener(_listenerId);

            _defaultWaypoints = defaultWaypointsFile
                .ParseAsMany<PredefinedWaypointTemplate>()
                .ToList();

            if (!_globalSettings.AlwaysLoadDefaultWaypoints)
            {
                OpenFirstRunDialogue();
            }
            else
            {
                SaveDefaultWaypointsToDisk();
            }
            //SaveDefaultWaypointsToDisk();
            _worldSettings.FirstRun = false;
        }
    }

    public void ResetToFactorySettings()
    {
        _worldSettings.FirstRun = true;

        _globalSettings.NeverLoadDefaultWaypoints = false;
        _globalSettings.AlwaysLoadDefaultWaypoints = false;

        _defaultWaypoints.Clear();

        _fileSystemService
            .GetJsonFile("waypoint-types.json")
            .SaveFrom(_defaultWaypoints);

        HandleFirstRun();
    }

    private void SaveDefaultWaypointsToDisk()
    {
        _fileSystemService
            .GetJsonFile("waypoint-types.json")
            .SaveFrom(_defaultWaypoints);
    }

    public void OpenFirstRunDialogue()
    {
        Capi.Event.EnqueueMainThreadTask(() =>
        {
            IOC.Services.CreateInstance<FirstRunDialogue>(new Action<bool, bool>((loadWaypoints, rememberSettings) =>
            {
                if (!loadWaypoints)
                {
                    _globalSettings.NeverLoadDefaultWaypoints = rememberSettings;
                    return;
                }
                _globalSettings.AlwaysLoadDefaultWaypoints = rememberSettings;

                SaveDefaultWaypointsToDisk();
            })).TryOpen();
        }, "waypoint-types");
    }
}