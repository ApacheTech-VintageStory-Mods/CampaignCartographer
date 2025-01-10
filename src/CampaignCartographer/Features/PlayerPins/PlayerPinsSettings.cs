using System.Drawing;

namespace ApacheTech.VintageMods.CampaignCartographer.Features.PlayerPins;

/// <summary>
///     A strongly-typed representation of the settings in the JSON file for the PlayerPins feature. This class cannot be inherited.
/// </summary>
[JsonObject]
public sealed class PlayerPinsSettings : FeatureSettings<PlayerPinsSettings>
{
    /// <summary>
    ///     Gets or sets the colour of the player's own pin.
    /// </summary>
    /// <value>The <see cref="Color"/> that the player's own pin will display as.</value>
    public Color SelfColour { get; set; } = Color.White;

    /// <summary>
    ///     Gets or sets the scale of the pin for the player.
    /// </summary>
    /// <value>An <see cref="int"/> value, from -5, to 20.</value>
    public int SelfScale { get; set; }

    /// <summary>
    ///     Gets or sets the colour of the pin for other server members that have been added as highlighted players.
    /// </summary>
    /// <value>The <see cref="Color"/> that the highlighted players' pin will display as.</value>
    public Color HighlightColour { get; set; } = Color.Cyan;

    /// <summary>
    ///     Gets or sets the scale of the pin for other server members that have been added as highlighted players.
    /// </summary>
    /// <value>An <see cref="int"/> value, from -5, to 20.</value>
    public int HighlightScale { get; set; }

    /// <summary>
    ///     Gets or sets the colour of the pin for other server members that haven't been added as highlighted players.
    /// </summary>
    /// <value>The <see cref="Color"/> that the other players' pin will display as.</value>
    public Color OthersColour { get; set; } = Color.FromArgb(76, 76, 76);

    /// <summary>
    ///     Gets or sets the scale of the pin for other server members that haven't been added as highlighted players.
    /// </summary>
    /// <value>An <see cref="int"/> value, from -5, to 20.</value>
    public int OthersScale { get; set; }

    /// <summary>
    ///     Gets a list of the people on the server that the user has added as highlighted players.
    /// </summary>
    /// <value>A <see cref="Dictionary{TKey,TValue}"/>, where the key is the player name, and the value is the PlayerUID.</value>
    public Dictionary<string, string> HighlightedPlayers { get; set; } = [];
}