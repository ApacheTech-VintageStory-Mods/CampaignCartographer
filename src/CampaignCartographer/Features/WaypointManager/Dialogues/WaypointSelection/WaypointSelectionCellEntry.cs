using ApacheTech.VintageMods.CampaignCartographer.Features.WaypointManager.WaypointTemplates;
using Gantry.Core.GameContent.GUI.Abstractions;

namespace ApacheTech.VintageMods.CampaignCartographer.Features.WaypointManager.Dialogues.WaypointSelection;

/// <summary>
///     Cell information, displayed within a <see cref="WaypointSelectionCellEntry"/>, in the cell list on the <see cref="WaypointSelectionDialogue"/> screen.
/// </summary>
/// <seealso cref="SavegameCellEntry" />
public class WaypointSelectionCellEntry : CellEntry<SelectableWaypointTemplate>
{
    /// <summary>
    ///     The position within the Waypoint list, regardless of sorting.
    /// </summary>
    public int Index { get; init; }
}