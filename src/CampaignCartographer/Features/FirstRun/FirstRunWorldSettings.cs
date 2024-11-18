namespace ApacheTech.VintageMods.CampaignCartographer.Features.FirstRun;

/// <summary>
///     General Per-World Settings for the Client.
/// </summary>
public class FirstRunWorldSettings
{
    /// <summary>
    ///     Gets or sets a value indicating whether this is the first time the user has run this mod on this server.
    /// </summary>
    /// <value><c>true</c> if it's the first time the mod has been run; otherwise, <c>false</c>.</value>
    public bool FirstRun { get; set; } = true;
}