using ApacheTech.Common.Extensions.Harmony;
using Gantry.Services.FileSystem.Abstractions.Contracts;
using ProperVersion;

namespace ApacheTech.VintageMods.CampaignCartographer.Features.WaypointManager.WaypointTemplates;

/// <summary>
///     Service for managing waypoint templates, including loading default templates and retrieving templates by key.
/// </summary>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public class WaypointTemplateService
{
    private readonly ICoreClientAPI _capi;
    private readonly IFileSystemService _fileSystemService;
    private List<PredefinedWaypointTemplate> _defaultWaypoints;

    /// <summary>
    ///     Gets the collection of waypoint templates indexed by their keys.
    /// </summary>
    private SortedDictionary<string, PredefinedWaypointTemplate> WaypointTemplates { get; } = [];

    /// <summary>
    ///     Initialises a new instance of the <see cref="WaypointTemplateService"/> class.
    /// </summary>
    /// <param name="capi">The client API for interacting with the core system.</param>
    /// <param name="fileSystemService">The service used for file operations.</param>
    public WaypointTemplateService(ICoreClientAPI capi, IFileSystemService fileSystemService)
    {
        _capi = capi;
        _fileSystemService = fileSystemService;
        LoadWaypointTemplates();
    }

    /// <summary>
    ///     Gets a string representing the available waypoint template syntax, joined by a delimiter.
    /// </summary>
    /// <returns>A string of waypoint template keys separated by " | ".</returns>
    public string GetSyntaxListText()
    {
        return string.Join(" | ", WaypointTemplates.Keys);
    }

    /// <summary>
    ///     Loads the waypoint templates from file and applies version checks.
    /// </summary>
    public void LoadWaypointTemplates()
    {
        try
        {
            WaypointTemplates.Clear();

            // Load default waypoints
            _defaultWaypoints = _fileSystemService
                .GetJsonFile("default-waypoints.json")
                .ParseAsMany<PredefinedWaypointTemplate>()
                .ToList();

            // Version check and update if needed
            var versionFile = _fileSystemService.GetJsonFile("version.data");
            var versionData = versionFile.ParseAsJsonObject();
            var version = versionData["Version"].AsString();
            var installedVersion = SemVer.Parse(version);

            if (installedVersion < SemVer.Parse(ModEx.ModInfo.Version))
            {
                var defaultWaypointsFile = _fileSystemService.GetJsonFile("default-waypoints.json");
                ApiEx.Logger.VerboseDebug("Updating global default files.");
                var globalConfigFile = _fileSystemService.GetJsonFile("version.data");
                defaultWaypointsFile.DisembedFrom(ModEx.ModAssembly);
                globalConfigFile.DisembedFrom(GetType().Assembly);
            }

            // Load waypoint templates from the configuration file
            var waypointsFile = _fileSystemService.GetJsonFile("waypoint-types.json");
            var waypoints = waypointsFile.ParseAsMany<PredefinedWaypointTemplate>();
            WaypointTemplates.AddOrUpdateRange(waypoints.Where(p => p.Enabled), p => p.Key);

            ApiEx.Logger.Event($"{WaypointTemplates.Count} waypoint extensions loaded.");
        }
        catch (Exception e)
        {
            ApiEx.Logger.Error("Error loading syntax for .wp command.");
            ApiEx.Logger.Error(e);
        }
    }

    /// <summary>
    ///     Retrieves a waypoint template by its key.
    /// </summary>
    /// <param name="key">The key of the waypoint template to retrieve.</param>
    /// <returns>
    ///     A cloned <see cref="PredefinedWaypointTemplate"/> if found, or <c>null</c> if no matching template exists.
    /// </returns>
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