using ApacheTech.VintageMods.CampaignCartographer.Features.ManualWaypoints;
using ApacheTech.VintageMods.CampaignCartographer.Features.ManualWaypoints.Systems;
using Gantry.Core.Extensions.DotNet;

namespace ApacheTech.VintageMods.CampaignCartographer.Features.WaypointManager.WaypointTemplates;

/// <summary>
///     Service for managing waypoint templates, including loading default templates and retrieving templates by key.
/// </summary>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public class WaypointTemplateService
{
    private readonly PredefinedWaypointsSettings _settings;

    /// <summary>
    ///     Gets the collection of waypoint templates indexed by their keys.
    /// </summary>
    private SortedDictionary<string, PredefinedWaypointTemplate> WaypointTemplates { get; } = [];

    /// <summary>
    ///     Initialises a new instance of the <see cref="WaypointTemplateService"/> class.
    /// </summary>
    /// <param name="capi">The client API for interacting with the core system.</param>
    /// <param name="fileSystemService">The service used for file operations.</param>
    public WaypointTemplateService(PredefinedWaypointsSettings settings)
    {
        _settings = settings;
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
            var disabledPacks = _settings.DisabledTemplatePacks;
            var templatePacks = PredefinedWaypoints.TemplatePacks.Where(p => !disabledPacks.Contains(p.Metadata.Name)).ToList();
            foreach (var templatePack in templatePacks)
            {
                var disabledTemplates = _settings.DisabledTemplatesPerPack.All(templatePack.Metadata.Name);
                var templates = templatePack.GetTemplates().Where(p => !disabledTemplates.Contains(p.Key));
                foreach (var template in templates)
                {
                    WaypointTemplates.AddIfNotPresent(template.Key, template);
                }
            }

            G.Logger.VerboseDebug($"{WaypointTemplates.Count} waypoint extensions loaded from {templatePacks.Count} template packs.");
        }
        catch (Exception e)
        {
            G.Logger.Error("Error loading syntax for .wp command.");
            G.Logger.Error(e);
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
        WaypointTemplates.TryGetValue(key, out var template);
        return template;
    }
}