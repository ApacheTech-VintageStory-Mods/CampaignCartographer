using ApacheTech.Common.Extensions.Harmony;
using Gantry.Services.FileSystem.Abstractions.Contracts;
using ProperVersion;

namespace ApacheTech.VintageMods.CampaignCartographer.Features.WaypointManager.WaypointTemplates;

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public class WaypointTemplateService
{
    private readonly ICoreClientAPI _capi;
    private readonly IFileSystemService _fileSystemService;
    private List<PredefinedWaypointTemplate> _defaultWaypoints;

    private SortedDictionary<string, PredefinedWaypointTemplate> WaypointTemplates { get; } = [];

    public WaypointTemplateService(ICoreClientAPI capi, IFileSystemService fileSystemService)
    {
        _capi = capi;
        _fileSystemService = fileSystemService;
        LoadWaypointTemplates();
    }

    public string GetSyntaxListText()
    {
        return string.Join(" | ", WaypointTemplates.Keys);
    }

    public void LoadWaypointTemplates()
    {
        try
        {
            WaypointTemplates.Clear();

            _defaultWaypoints = _fileSystemService
                .GetJsonFile("default-waypoints.json")
                .ParseAsMany<PredefinedWaypointTemplate>()
                .ToList();

            var versionFile = _fileSystemService.GetJsonFile("version.data");
            var versionData = versionFile.ParseAsJsonObject();
            var version = versionData["Version"].AsString();
            var installedVersion = SemVer.Parse(version);

            if (installedVersion < SemVer.Parse(ModEx.ModInfo.Version))
            {
                var defaultWaypointsFile = _fileSystemService.GetJsonFile("default-waypoints.json");
                _capi.Logger.VerboseDebug("Campaign Cartographer: Updating global default files.");
                var globalConfigFile = _fileSystemService.GetJsonFile("version.data");
                defaultWaypointsFile.DisembedFrom(ModEx.ModAssembly);
                globalConfigFile.DisembedFrom(GetType().Assembly);
            }

            var waypointsFile = _fileSystemService.GetJsonFile("waypoint-types.json");
            var waypoints = waypointsFile.ParseAsMany<PredefinedWaypointTemplate>();
            WaypointTemplates.AddOrUpdateRange(waypoints.Where(p => p.Enabled), p => p.Key);

            _capi.Logger.Event($"{WaypointTemplates.Count} waypoint extensions loaded.");
        }
        catch (Exception e)
        {
            _capi.Logger.Error($"Waypoint Extensions: Error loading syntax for .wp command; {e.Message}");
            _capi.Logger.Error(e.StackTrace);
        }
    }

    public PredefinedWaypointTemplate GetTemplateByKey(string key)
    {
        if (WaypointTemplates.TryGetValue(key, out var template))
        {
            return template.Clone() as PredefinedWaypointTemplate;
        }

        if (_defaultWaypoints is null || _defaultWaypoints.Count == 0) return null;

        return _defaultWaypoints
            .Where(p => p.Key.Equals(key, StringComparison.InvariantCultureIgnoreCase))
            .Select(p => p.DeepClone())
            .FirstOrNull();
    }
}