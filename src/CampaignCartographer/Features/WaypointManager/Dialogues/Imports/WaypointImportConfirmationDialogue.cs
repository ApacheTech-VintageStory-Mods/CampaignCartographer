using System.Collections.Immutable;
using ApacheTech.VintageMods.CampaignCartographer.Features.WaypointManager.Dialogues.WaypointSelection;
using ApacheTech.VintageMods.CampaignCartographer.Features.WaypointManager.Repositories;
using ApacheTech.VintageMods.CampaignCartographer.Features.WaypointManager.WaypointTemplates;
using Gantry.Core.GameContent.GUI.Abstractions;
using Gantry.Core.GameContent.GUI.Models;

namespace ApacheTech.VintageMods.CampaignCartographer.Features.WaypointManager.Dialogues.Imports;


/// <summary>
///     Dialogue Window: Allows the user to export waypoints to JSON files.
/// </summary>
/// <seealso cref="GenericDialogue" />
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public class WaypointImportConfirmationDialogue : WaypointSelectionDialogue
{
    private readonly IEnumerable<ImportedWaypointTemplate> _waypoints;
    private readonly IEnumerable<WaypointSelectionCellEntry> _cells;
    private ImmutableSortedDictionary<int, Waypoint> _sortedWaypoints = ImmutableSortedDictionary<int, Waypoint>.Empty;

    [ActivatorUtilitiesConstructor]
    public WaypointImportConfirmationDialogue(ICoreClientAPI capi, List<ImportedWaypointTemplate> waypoints) : base(capi)
    {
        Title = T("ImportConfirmation.Title");
        Alignment = EnumDialogArea.CenterMiddle;
        Modal = true;
        ModalTransparency = 0f;
        _waypoints = waypoints.Select(WithRelativePosition);

        ImportedWaypointTemplate WithRelativePosition(ImportedWaypointTemplate template)
        {
            var originalPosition = template.Position;
            var absoluteOnFile = originalPosition.ToAbsolute(template.SpawnPosition.XYZ);
            var relativeInWorld = absoluteOnFile.RelativeToSpawn();
            var absoluteInWorld = relativeInWorld.ToAbsolute();
            return template.With(p => p.Position = absoluteInWorld);
        }

        _cells = CreateCellEntries();
    }

    private IEnumerable<WaypointSelectionCellEntry> CreateCellEntries()
    {
        var world = ApiEx.Client.World;
        var playerPos = world.Player.Entity.Pos.AsBlockPos;

        string RightTopText(Waypoint waypoint)
        {
            var relativePosition = waypoint.Position.AsBlockPos.RelativeToSpawn();
            var distance = waypoint.Position.AsBlockPos.HorizontalManhattenDistance(playerPos);
            var text = $"{relativePosition} ({distance:N2}m)";
            return text;
        }

        _sortedWaypoints = WaypointQueriesRepository
            .SortWaypoints(_waypoints, SortOrder)
            .ToImmutableSortedDictionary();

        var list = _sortedWaypoints.Select(dto =>
        {
            var w = new WaypointSelectionCellEntry
            {
                Title = dto.Value.Title,
                RightTopText = RightTopText(dto.Value),
                RightTopOffY = 3f,
                DetailTextFont = CairoFont.WhiteDetailText().WithFontSize((float)GuiStyle.SmallFontSize),
                Model = SelectableWaypointTemplate.FromWaypoint(dto.Value, selected: true),
                Index = dto.Key
            };

            return w;
        });
        return list;
    }

    protected override IEnumerable<WaypointSelectionCellEntry> GetCellEntries(System.Func<SelectableWaypointTemplate, bool> filter) 
        => WaypointQueriesRepository.SortCells(SortOrder, _cells);

    #region Primary Button: Export Selected Waypoints

    protected override string PrimaryButtonText => T("ImportConfirmation.RightButtonText");

    protected override ActionConsumable PrimaryButtonAction => ImportSelectedWaypoints;

    private bool ImportSelectedWaypoints()
    {
        if (CellList is null) return false;
        var waypoints = CellList.elementCells
            .Cast<WaypointSelectionGuiCell>()
            .Where(p => p.On)
            .Select(w => w.Model)
            .ToList();

        var code = LangEx.FeatureCode("WaypointManager.Dialogue.Imports", "File");
        var pluralisedFile = LangEx.Pluralise(code, waypoints.Count);
        var totalCount = waypoints.Count;

        var title = T("ImportConfirmation.ConfirmationTitle");
        var message = T("ImportConfirmation.ConfirmationMessage", totalCount.ToString("N0"), pluralisedFile);

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

    #region Secondary Button: Open Exports Folder

    protected override string SecondaryButtonText => T("ImportConfirmation.LeftButtonText");

    protected override ActionConsumable SecondaryButtonAction => BackToWaypointManager;

    private bool BackToWaypointManager()
    {
        var dialogue = IOC.Services.GetRequiredService<WaypointImportDialogue>();
        while (dialogue.IsOpened(dialogue.ToggleKeyCombinationCode))
            dialogue.TryClose();
        dialogue.TryOpen();
        return TryClose();
    }

    #endregion
}