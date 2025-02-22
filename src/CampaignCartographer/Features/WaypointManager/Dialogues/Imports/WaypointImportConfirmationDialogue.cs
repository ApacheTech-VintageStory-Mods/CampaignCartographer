using ApacheTech.VintageMods.CampaignCartographer.Features.WaypointManager.Dialogues.WaypointSelection;
using ApacheTech.VintageMods.CampaignCartographer.Features.WaypointManager.Repositories;
using ApacheTech.VintageMods.CampaignCartographer.Features.WaypointManager.WaypointTemplates;
using Gantry.Core;
using Gantry.Core.GameContent.GUI.Abstractions;
using Gantry.Core.GameContent.GUI.Models;
using Gantry.Core.Hosting.Annotation;

namespace ApacheTech.VintageMods.CampaignCartographer.Features.WaypointManager.Dialogues.Imports;

/// <summary>
///     Dialogue Window: Allows the user to export waypoints to JSON files.
/// </summary>
/// <seealso cref="GenericDialogue" />
public class WaypointImportConfirmationDialogue : WaypointSelectionDialogue
{
    private readonly WaypointQueriesRepository _queriesRepo;
    private readonly List<ImportedWaypointTemplate> _waypoints;

    /// <summary>
    /// 	Initialises a new instance of the <see cref="WaypointImportConfirmationDialogue" /> class.
    /// </summary>
    /// <param name="capi">Client API pass-through</param>
    /// <param name="queriesRepo">IOC Injected Waypoint Query Repository.</param>
    /// <param name="waypoints"></param>
    [SidedConstructor]
    public WaypointImportConfirmationDialogue(
        ICoreClientAPI capi, WaypointQueriesRepository queriesRepo, List<ImportedWaypointTemplate> waypoints) : base(capi, queriesRepo)
    {
        _queriesRepo = queriesRepo;
        _waypoints = waypoints;
        Title = LangEx.FeatureString("WaypointManager.Dialogue.ImportConfirmation", "Title");
        Alignment = EnumDialogArea.CenterMiddle;
        Modal = true;
        ModalTransparency = 0f;
        LeftButtonText = LangEx.FeatureString("WaypointManager.Dialogue.ImportConfirmation", "LeftButtonText");
        RightButtonText = LangEx.FeatureString("WaypointManager.Dialogue.ImportConfirmation", "RightButtonText");
        ShowExtraButtons = false;

        ClientSettings.Inst.AddWatcher<float>("guiScale", _ =>
        {
            Compose();
            RefreshValues();
        });
    }

    public static WaypointImportConfirmationDialogue Create(List<ImportedWaypointTemplate> waypoints)
    {
        return IOC.Services.CreateInstance<WaypointImportConfirmationDialogue>(waypoints);
    }

    #region Form Composition

    protected override void PopulateCellList()
    {
        Waypoints = GetWaypointImportCellEntries();
        base.PopulateCellList();
    }

    #endregion

    #region Cell List Management

    private List<WaypointSelectionCellEntry> GetWaypointImportCellEntries()
    {
        var world = ApiEx.Client.World;
        var playerPos = world.Player.Entity.Pos.AsBlockPos;

        ImportedWaypointTemplate WithRelativePosition(ImportedWaypointTemplate template)
        {
            var originalPosition = template.Position;
            var absoluteOnFile = originalPosition.ToAbsolute(template.SpawnPosition.XYZ);
            var relativeInWorld = absoluteOnFile.RelativeToSpawn();
            var absoluteInWorld = relativeInWorld.ToAbsolute();
            return template.With(p => p.Position = absoluteInWorld);
        }

        var waypoints = _queriesRepo.SortWaypoints(_waypoints.Select(WithRelativePosition), SortOrder);      

        var list = waypoints.Select(dto =>
        {
            var w = new WaypointSelectionCellEntry
            {
                Title = dto.Value.Title,
                RightTopText = $"{dto.Value.Position.AsBlockPos.RelativeToSpawn()} ({dto.Value.Position.AsBlockPos.HorizontalManhattenDistance(playerPos):N2}m)",
                RightTopOffY = 3f,
                DetailTextFont = CairoFont.WhiteDetailText().WithFontSize((float)GuiStyle.SmallFontSize),
                Model = SelectableWaypointTemplate.FromWaypoint(dto.Value, selected: true),
                Index = dto.Key
            };

            return w;
        }).ToList();
        return list;
    }

    #endregion

    #region Control Event Handlers

    protected override bool OnLeftButtonPressed()
    {
        var dialogue = IOC.Services.Resolve<WaypointImportDialogue>();
        while (dialogue.IsOpened(dialogue.ToggleKeyCombinationCode))
            dialogue.TryClose();
        dialogue.TryOpen();
        return TryClose();
    }

    protected override bool OnRightButtonPressed()
    {
        var waypoints = WaypointsList.elementCells      
            .Cast<WaypointSelectionGuiCell>()
            .Where(p => p.On)
            .Select(w => w.Model)
            .ToList();

        var code = LangEx.FeatureCode("WaypointManager.Dialogue.Imports", "File");
        var pluralisedFile = LangEx.Pluralise(code, waypoints.Count);
        var totalCount = waypoints.Count;

        var title = LangEx.FeatureString("WaypointManager.Dialogue.ImportConfirmation", "ConfirmationTitle");
        var message = LangEx.FeatureString("WaypointManager.Dialogue.ImportConfirmation", "ConfirmationMessage",
            totalCount.ToString("N0"), pluralisedFile);

        MessageBox.Show(title, message, ButtonLayout.OkCancel, () =>
        {
            IOC.Services
                .Resolve<WaypointCommandsRepository>()
                .AddWaypoints(waypoints);
            TryClose();
        });
        return true;
    }

    #endregion
}