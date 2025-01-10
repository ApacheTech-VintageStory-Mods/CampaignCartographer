using System.IO;
using ApacheTech.VintageMods.CampaignCartographer.Features.WaypointManager.Dialogues.WaypointSelection;
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
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public class WaypointExportDialogue : WaypointSelectionDialogue
{
    private readonly WaypointQueriesRepository _queriesRepo;

    private readonly string _exportsDirectory =
        ModPaths.CreateDirectory(Path.Combine(ModPaths.ModDataWorldPath, "Saves"));

    /// <summary>
    /// 	Initialises a new instance of the <see cref="WaypointExportDialogue" /> class.
    /// </summary>
    /// <param name="capi">Client API pass-through</param>
    /// <param name="queriesRepo">IOC Injected Waypoint Service.</param>
    public WaypointExportDialogue(ICoreClientAPI capi, WaypointQueriesRepository queriesRepo) : base(capi, queriesRepo)
    {
        _queriesRepo = queriesRepo;
        Title = LangEx.FeatureString("WaypointManager.Dialogue", "Title");
        Alignment = EnumDialogArea.CenterMiddle;
        Modal = true;
        ModalTransparency = 0f;
        LeftButtonText = LangEntry("OpenExportsFolder");
        RightButtonText = LangEntry("ExportSelectedWaypoints");
        ShowTopRightButton = true;

        ClientSettings.Inst.AddWatcher<float>("guiScale", _ =>
        {
            Compose();
            RefreshValues();
        });
    }

    #region Form Composition

    protected override void PopulateCellList()
    {
        Waypoints = GetWaypointExportCellEntries();
        base.PopulateCellList();
    }

    #endregion

    #region Cell List Management

    private List<WaypointSelectionCellEntry> GetWaypointExportCellEntries()
    {
        var playerPos = ApiEx.Client.World.Player.Entity.Pos.AsBlockPos;
        var waypoints = _queriesRepo.GetSortedWaypoints(SortOrder, w => SelectableWaypointTemplate.FromWaypoint(w, selected: false));
        return waypoints.Select(w =>
        {
            var dto = w.Value;
            var current = Waypoints.FirstOrDefault(p => p.Model.Equals(w.Value));
            dto.Selected = current?.Model.Selected ?? true;

            return new WaypointSelectionCellEntry
            {
                Title = dto.Title,
                RightTopText = $"{dto.Position.AsBlockPos.RelativeToSpawn()} ({dto.Position.AsBlockPos.HorizontalManhattenDistance(playerPos):N2}m)",
                RightTopOffY = 3f,
                DetailTextFont = CairoFont.WhiteDetailText().WithFontSize((float)GuiStyle.SmallFontSize),
                Model = dto,
                Index = w.Key
            };
        }).ToList();
    }

    #endregion

    #region Control Event Handlers

    protected override bool OnLeftButtonPressed()
    {
        NetUtil.OpenUrlInBrowser(_exportsDirectory);
        return true;
    }

    protected override bool OnRightButtonPressed()
    {
        var waypoints = WaypointsList.elementCells
            .Cast<WaypointSelectionGuiCell>()
            .Where(p => p.On)
            .Select(w => (PositionedWaypointTemplate)w.Model)
            .ToList();
        WaypointExportConfirmationDialogue.ShowDialogue(waypoints);
        return true;
    }

    #endregion
}