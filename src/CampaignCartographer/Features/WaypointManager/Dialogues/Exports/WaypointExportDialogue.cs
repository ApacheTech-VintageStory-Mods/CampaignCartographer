using ApacheTech.VintageMods.CampaignCartographer.Features.WaypointManager.Dialogues.Imports;
using ApacheTech.VintageMods.CampaignCartographer.Features.WaypointManager.Dialogues.WaypointSelection;
using ApacheTech.VintageMods.CampaignCartographer.Features.WaypointManager.Extensions;
using ApacheTech.VintageMods.CampaignCartographer.Features.WaypointManager.Repositories;
using ApacheTech.VintageMods.CampaignCartographer.Features.WaypointManager.WaypointTemplates;
using Gantry.Core.GameContent.GUI.Abstractions;
using Gantry.Services.FileSystem;
using Vintagestory.API.Util;

namespace ApacheTech.VintageMods.CampaignCartographer.Features.WaypointManager.Dialogues.Exports;

/// <summary>
///     Dialogue Window: Allows the user to export waypoints to JSON files.
/// </summary>
/// <seealso cref="GenericDialogue" />
[HarmonyClientSidePatch]
public class WaypointExportDialogue : WaypointSelectionDialogue
{
    private readonly IPlayer[] _onlinePlayers;
    private static string _searchTerm;

    public WaypointExportDialogue(ICoreClientAPI capi) : base(capi)
    {
        Title = T("Title");
        Modal = true;
        ModalTransparency = 0f;

        _onlinePlayers = [.. capi.World.AllOnlinePlayers.Except([capi.World.Player])];
    }
    
    protected override string SearchTerm { get => _searchTerm; set => _searchTerm = value; }

    #region Cell Management

    private static WaypointExportDialogue _instance;

    protected override IEnumerable<WaypointSelectionCellEntry> GetCellEntries(System.Func<SelectableWaypointTemplate, bool> filter)
    {
        // Retrieve the player's position.
        var playerPos = ApiEx.Client.World.Player.Entity.Pos.AsBlockPos;

        // Convert the list of waypoints to a sorted dictionary.
        
        var query = IOC.Services.GetRequiredService<WaypointQueriesRepository>();
        var waypoints = query.GetSortedWaypoints(SortOrder, w => SelectableWaypointTemplate.FromWaypoint(w, selected: true));

        // Sort the waypoints and convert each to a selectable template,
        // filtering out any null conversions.
        return waypoints.Where(p => filter(p.Value)).Select(w =>
        {
            var dto = w.Value;

            // Check for any critical null values; if any are found, skip this entry.
            if (dto is null || dto.Title is null || dto.Position is null) return null;

            // Check the relative position; if unavailable, skip this entry.
            var relativePos = dto.Position.AsBlockPos.RelativeToSpawn();
            if (relativePos is null) return null;

            return new WaypointSelectionCellEntry
            {
                Title = dto.Title,
                RightTopText = $"{relativePos} ({dto.Position.AsBlockPos.HorizontalManhattenDistance(playerPos):N2}m)",
                RightTopOffY = 3f,
                DetailTextFont = CairoFont.WhiteDetailText().WithFontSize((float)GuiStyle.SmallFontSize),
                Model = dto,
                Index = w.Key
            };
        })
        .Where(entry => entry is not null);
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(WaypointMapLayer), "OnDataFromServer")]
    public static void UpdateWaypointExportDialogueCells() => _instance?.RecomposeBody();

    public override bool TryOpen()
    {
        var success = base.TryOpen();
        if (success) _instance = this;
        return success;
    }

    public override bool TryClose()
    {
        var success = base.TryClose();
        if (success) _instance = null;
        return success;
    }

    #endregion

    #region Primary Button: Export Selected Waypoints

    protected override string PrimaryButtonText => T("Exports.ExportSelectedWaypoints");

    protected override ActionConsumable PrimaryButtonAction => ExportSelectedWaypoints;

    private bool ExportSelectedWaypoints()
    {
        var waypoints = CellList.elementCells
            .Cast<WaypointSelectionGuiCell>()
            .Where(p => p.On)
            .Select(w => (PositionedWaypointTemplate)w.Model)
            .ToList();
        WaypointExportConfirmationDialogue.ShowDialogue(waypoints);
        return true;
    }

    #endregion

    #region Secondary Button: Open Exports Folder

    protected override string SecondaryButtonText => T("Exports.OpenExportsFolder");

    protected override ActionConsumable SecondaryButtonAction => OpenExportsFolder;

    private bool OpenExportsFolder()
    {
        var folder = ModPaths.CreateDirectory(System.IO.Path.Combine(ModPaths.ModDataWorldPath, "Saves"));
        NetUtil.OpenUrlInBrowser(folder);
        return true;
    }

    #endregion

    #region Tertiary Button: Share Waypoint

    protected override string TertiaryButtonText => T("Exports.ShareSelected");

    protected override ActionConsumable TertiaryButtonAction => _onlinePlayers.Length > 0 ? ShareSelectedWaypoints : null;

    private bool ShareSelectedWaypoints()
    {
        var cellList = CellList.elementCells.Cast<WaypointSelectionGuiCell>().Where(p => p.On);
        var waypoints = cellList.Select(p => p.Model.ToWaypoint());
        var dialogue = new ShareWaypointDialogue(capi, _onlinePlayers, waypoints);
        dialogue.ToggleGui();
        return true;
    }

    #endregion

    #region Header Button: Import Waypoints

    protected override string HeaderButtonText => T("Imports.Title");

    protected override ActionConsumable HeaderButtonAction => OnImportButtonClicked;

    private bool OnImportButtonClicked()
    {
        var dialogue = IOC.Services.GetRequiredService<WaypointImportDialogue>();
        dialogue.OnClosed += () => TryClose();
        return dialogue.TryOpen();
    }

    #endregion
}