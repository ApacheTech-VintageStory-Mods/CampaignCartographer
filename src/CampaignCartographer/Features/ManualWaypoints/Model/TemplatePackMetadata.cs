using Gantry.Services.FileSystem.Enums;
using Newtonsoft.Json.Converters;

namespace ApacheTech.VintageMods.CampaignCartographer.Features.ManualWaypoints.Model;

/// <summary>
///     Contains metadata information for a <see cref="TemplatePack"/>.
/// </summary>
[JsonObject]
public class TemplatePackMetadata
{
    /// <summary>
    ///     The name of the template pack.
    /// </summary>
    public string Name { get; set; } = "";

    /// <summary>
    ///     The title of the template pack.
    /// </summary>
    [JsonIgnore]
    public string Title => LangEx.Get($"TemplatePacks.{Name}.Title");

    /// <summary>
    ///     A brief description of the template pack.
    /// </summary>
    [JsonIgnore]
    public string Description => LangEx.Get($"TemplatePacks.{Name}.Description");

    /// <summary>
    ///     The version of the template pack.
    /// </summary>
    public Version Version { get; set; } = new Version(1, 0, 0);

    /// <summary>
    ///     Determines whether the pack is enabled.
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    ///     Determines whether the user can add templates to the pack.
    /// </summary>
    public bool Custom { get; set; } = true;

    /// <summary>
    ///     Determines the scope of the templates within the pack.
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public FileScope Scope { get; set; } = FileScope.Global;
}