namespace ApacheTech.VintageMods.CampaignCartographer.Features.Waypoints.Automatic;

/// <summary>
///     Represents dictionaries that map between block codes and waypoint types.
/// </summary>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public class CrossMaps
{
    /// <summary>
    ///     Gets or sets the dictionary that maps ore block codes to their corresponding waypoint types.
    /// </summary>
    public Dictionary<string, string> Ores { get; init; }

    /// <summary>
    ///     Gets or sets the dictionary that maps stone block codes to their corresponding waypoint types.
    /// </summary>
    public Dictionary<string, string> Stones { get; init; }

    /// <summary>
    ///     Gets or sets the dictionary that maps organic block codes to their corresponding waypoint types.
    /// </summary>
    public Dictionary<string, string> Organics { get; init; }
}
