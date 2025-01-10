using ApacheTech.VintageMods.CampaignCartographer.Features.FastTravelOverlay.Models;
using Gantry.Core.GameContent.AssetEnum;

namespace ApacheTech.VintageMods.CampaignCartographer.Features.FastTravelOverlay;

/// <summary>
///     Contains the settings for the fast travel overlay.
/// </summary>
public class FastTravelOverlaySettings : FeatureSettings<FastTravelOverlaySettings>
{
    /// <summary>
    ///     The colour of the translocator, represented as a hexadecimal string.
    /// </summary>
    public string TranslocatorColour { get; set; } = NamedColour.Purple;

    /// <summary>
    ///     The colour of the teleporter, represented as a hexadecimal string.
    /// </summary>
    public string TeleporterColour { get; set; } = NamedColour.Teal;

    /// <summary>
    ///     The colour for errors, represented as a hexadecimal string.
    /// </summary>
    public string ErrorColour { get; set; } = NamedColour.Red;

    /// <summary>
    ///     The opacity for error elements in the overlay.
    /// </summary>
    public int ErrorOpacity { get; set; } = 100;

    /// <summary>
    ///     The colour of disabled elements, represented as a hexadecimal string.
    /// </summary>
    public string DisabledColour { get; set; } = NamedColour.DarkGrey;

    /// <summary>
    ///     The opacity for disabled elements in the overlay.
    /// </summary>
    public int DisabledOpacity { get; set; } = 40;

    /// <summary>
    ///     The width of the path connecting fast travel nodes.
    /// </summary>
    public int PathWidth { get; set; } = 2;

    /// <summary>
    ///     The size of the fast travel node.
    /// </summary>
    public int NodeSize { get; set; } = 3;

    /// <summary>
    ///     The list of fast travel nodes associated with the overlay.
    /// </summary>
    public List<FastTravelOverlayNode> Nodes { get; set; } = [];
}