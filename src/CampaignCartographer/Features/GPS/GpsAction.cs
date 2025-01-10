namespace ApacheTech.VintageMods.CampaignCartographer.Features.GPS;

/// <summary>
///     Represents the various actions that can be performed with a GPS hyperlink.
/// </summary>
/// <remarks>
///     The actions define how the GPS hyperlink will be processed, such as notifying the user,
///     broadcasting a message, or copying the GPS link to the clipboard.
/// </remarks>
public enum GpsAction
{
    /// <summary>
    ///     Displays a notification to the user containing the GPS information.
    /// </summary>
    Notification,

    /// <summary>
    ///     Broadcasts the GPS information to all players or a specific group.
    /// </summary>
    Broadcast,

    /// <summary>
    ///     Copies the GPS link to the clipboard for external use or sharing.
    /// </summary>
    Clipboard
}