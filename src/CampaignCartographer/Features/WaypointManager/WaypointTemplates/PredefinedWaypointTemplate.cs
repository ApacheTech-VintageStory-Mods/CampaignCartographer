using ApacheTech.VintageMods.CampaignCartographer.Features.ManualWaypoints.Model;

namespace ApacheTech.VintageMods.CampaignCartographer.Features.WaypointManager.WaypointTemplates;

/// <summary>
///     Represents a Waypoint that the user can add to the map by used a specific key.
/// </summary>
[JsonObject]
[ProtoContract]
public class PredefinedWaypointTemplate : CoverageWaypointTemplate
{
    /// <summary>
    ///     The syntax the user must type to add this type of waypoint.
    /// </summary>
    /// <value>The syntax value of the waypoint.</value>
    [JsonRequired]
    [ProtoMember(9)]
    public string Key { get; set; } = string.Empty;

    /// <summary>
    ///     Determines whether this waypoint type is enabled for manual waypoint addition.
    /// </summary>
    /// <returns><c>true</c> if this waypoint type should be added to the manual waypoints syntax list; otherwise <c>false</c></returns>
    [ProtoMember(10)]
    public bool Enabled { get; set; } = true;

    /// <summary>
    ///     The title to add to the waypoint, translated into the user's language.
    /// </summary>
    [JsonIgnore]
    public string TranslatedTitle => LangEx.Get($"Templates.{Key}.Title");

    /// <summary>
    ///     The template pack this template belongs to.
    /// </summary>
    [JsonIgnore]
    public TemplatePack TemplatePack { get; set; } = default!;

    /// <summary>
    ///     Gets the title of the template, returning the custom title if applicable; otherwise, returning the translated title.
    /// </summary>
    public new string Title
    {
        get => TemplatePack.Metadata.Custom ? _title : TranslatedTitle;
        set => _title = value;
    }

}