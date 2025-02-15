using ApacheTech.VintageMods.CampaignCartographer.Features.WaypointManager.WaypointTemplates;
using Gantry.Core.Extensions.DotNet;

namespace ApacheTech.VintageMods.CampaignCartographer.Features.ManualWaypoints.Model;

/// <summary>
///     Represents a collection of predefined waypoint templates with associated metadata.
/// </summary>
[JsonObject]
public class TemplatePack
{
    /// <summary>
    ///     Metadata describing the template pack.
    /// </summary>
    public TemplatePackMetadata Metadata { get; set; } = new();

    /// <summary>
    ///     The collection of predefined waypoint templates included in the pack.
    /// </summary>
    public List<PredefinedWaypointTemplate> Templates { get; set; } = [];

    public IEnumerable<PredefinedWaypointTemplate> GetTemplates() => Templates.EachWith(p => p.TemplatePack = this);
}