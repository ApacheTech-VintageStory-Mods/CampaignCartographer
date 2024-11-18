using Gantry.Core.GameContent.GUI.Abstractions;
using ApacheTech.VintageMods.CampaignCartographer.Features.WaypointManager.Models;

namespace ApacheTech.VintageMods.CampaignCartographer.Features.WaypointManager.Dialogues.Imports;

/// <summary>
///     Defines the information stored within a single GUI Cell Element.
///     A list of these cells is displayed in <see cref="WaypointImportDialogue"/>,
///     to allow the user to import waypoints from a JSON file.
/// </summary>
/// <seealso cref="SavegameCellEntry" />
public class WaypointImportCellEntry : CellEntry<WaypointFileModel>
{
}