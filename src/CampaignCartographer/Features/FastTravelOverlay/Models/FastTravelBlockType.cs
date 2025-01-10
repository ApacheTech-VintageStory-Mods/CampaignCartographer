namespace ApacheTech.VintageMods.CampaignCartographer.Features.FastTravelOverlay.Models;

/// <summary>
///     Represents the type of block used for fast travel in the game.
/// </summary>
public enum FastTravelBlockType
{
    /// <summary>
    ///     The type of the block is unknown.
    /// </summary>
    Unknown,

    /// <summary>
    ///     A static translocator.
    /// </summary>
    Translocator,

    /// <summary>
    ///     A teleporter block.
    /// </summary>
    Teleporter
}