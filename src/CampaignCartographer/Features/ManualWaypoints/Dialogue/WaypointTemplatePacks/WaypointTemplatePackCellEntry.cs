using Gantry.Core.GameContent.GUI.Abstractions;
using ApacheTech.VintageMods.CampaignCartographer.Features.ManualWaypoints.Model;

namespace ApacheTech.VintageMods.CampaignCartographer.Features.ManualWaypoints.Dialogue.WaypointTemplatePacks;

/// <summary>
///     Defines the information stored within a single GUI Cell Element.
///     A list of these cells is displayed in <see cref="WaypointImportDialogue"/>,
///     to allow the user to import waypoints from a JSON file.
/// </summary>
/// <seealso cref="SavegameCellEntry" />
public class WaypointTemplatePackCellEntry : CellEntry<TemplatePack>
{
}