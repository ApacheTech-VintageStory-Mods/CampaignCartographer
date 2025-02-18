namespace ApacheTech.VintageMods.CampaignCartographer.Features.FastTravelOverlay.Extensions;

/// <summary>
///     Provides extension methods for <see cref="TeleporterLocation"/> and related functionality.
/// </summary>
public static class TeleporterLocationExtensions
{
    /// <summary>
    ///     Determines whether the teleporter location has a valid target position.
    /// </summary>
    /// <param name="location">The teleporter location to check.</param>
    /// <returns>True if the location has a target position; otherwise, false.</returns>
    public static bool HasTargetLocation(this TeleporterLocation location)
        => location?.TargetPos is not null;
}