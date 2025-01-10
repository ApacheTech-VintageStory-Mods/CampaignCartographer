namespace ApacheTech.VintageMods.CampaignCartographer.Features.FastTravelOverlay.Models;

/// <summary>
///     Represents a fast travel node.
/// </summary>
public class FastTravelOverlayNode
{
    /// <summary>
    ///     Initialises a new instance of the <see cref="FastTravelOverlayNode"/> class.
    /// </summary>
    /// <param name="other">The instance to copy.</param>
    public static FastTravelOverlayNode CloneFrom(FastTravelOverlayNode other) => new()
    {
        Location = other.Location,
        Type = other.Type,
        Enabled = other.Enabled,
        ShowPath = other.ShowPath,
        NodeColour = other.NodeColour
    };

    /// <summary>
    ///     The position of the node.
    /// </summary>
    public TeleporterLocation Location { get; set; } = new();

    /// <summary>
    ///     The type of fast travel block associated with the node.
    /// </summary>
    public FastTravelBlockType Type { get; set; } = FastTravelBlockType.Unknown;

    /// <summary>
    ///     Indicates whether the fast travel node is enabled.
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    ///     Indicates whether to display the path to the fast travel node.
    /// </summary>
    public bool ShowPath { get; set; } = true;

    /// <summary>
    ///     The node colour, represented as a hexadecimal string.
    /// </summary>
    public string NodeColour { get; set; } = string.Empty;
}