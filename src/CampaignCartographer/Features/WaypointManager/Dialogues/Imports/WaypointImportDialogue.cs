﻿using System.IO;
using System.Text;
using ApacheTech.Common.Extensions.Harmony;
using ApacheTech.VintageMods.CampaignCartographer.Features.WaypointManager.Models;
using ApacheTech.VintageMods.CampaignCartographer.Features.WaypointManager.WaypointTemplates;
using Gantry.Core.GameContent.GUI.Abstractions;
using Gantry.Services.FileSystem;
using Vintagestory.API.Util;

namespace ApacheTech.VintageMods.CampaignCartographer.Features.WaypointManager.Dialogues.Imports;

/// <summary>
///     Dialogue Window: Allows the user to import waypoints from JSON files.
/// </summary>
/// <seealso cref="GenericDialogue" />
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public class WaypointImportDialogue : GenericDialogue
{
    private readonly FileSystemWatcher _watcher;
    private int _watcherLock;

    private ElementBounds _clippedBounds = ElementBounds.Fixed(0, 0, 0, 0);
    private ElementBounds _cellListBounds = ElementBounds.Fixed(0, 0, 0, 0);

    private List<WaypointImportCellEntry> _files = [];

    private GuiElementCellList<WaypointImportCellEntry> _filesList = null!;
    private GuiElementDynamicText _lblSelectedCount = null!;

    private readonly string _importsDirectory = ModPaths.CreateDirectory(Path.Combine(ModPaths.ModDataWorldPath, "Saves"));

    /// <summary>
    /// 	Initialises a new instance of the <see cref="WaypointImportDialogue" /> class.
    /// </summary>
    /// <param name="capi">Client API pass-through</param>
    public WaypointImportDialogue(ICoreClientAPI capi) : base(capi)
    {
        Title = LangEx.FeatureString("WaypointManager.Dialogue.Imports", "Title");
        Alignment = EnumDialogArea.CenterMiddle;

        _watcher = new FileSystemWatcher(_importsDirectory, "*.json");
        _watcher.Changed += OnDirectoryChanged;
        _watcher.Created += OnDirectoryChanged;
        _watcher.Deleted += OnDirectoryChanged;
        _watcher.Renamed += OnDirectoryChanged;
    }

    #region Form Composition

    /// <summary>
    ///     Composes the GUI components for this instance.
    /// </summary>
    protected override void Compose()
    {
        base.Compose();
        RefreshFiles();
    }

    /// <summary>
    ///     Refreshes the file list, displayed on the form, whenever changes are made.
    /// </summary>
    private void RefreshFiles()
    {
        _files = GetImportCellsFromDirectory(_importsDirectory);
        _filesList.ReloadCells(_files);
        _watcherLock = 0;
        _watcher.EnableRaisingEvents = true;
    }

    private static List<WaypointImportCellEntry> GetImportCellsFromDirectory(string path)
    {
        var files = new DirectoryInfo(path).GetFiles("*.json").OrderBy(p => p.CreationTime).ToList();
        var list = new List<WaypointImportCellEntry>();
        foreach (var file in files)
        {
            try
            {
                var dto = JsonConvert.DeserializeObject<WaypointFileModel>(File.ReadAllText(file.FullName));
                var detailText = new StringBuilder();
                var waypointCode = LangEx.FeatureCode("WaypointManager.Dialogue.Imports", "Waypoint");
                detailText.AppendLine(LangEx.FeatureString("WaypointManager.Dialogue.Imports", "FileText", file.Name));
                detailText.AppendLine(LangEx.FeatureString("WaypointManager.Dialogue.Imports", "CreatedText", dto!.DateCreated));
                detailText.AppendLine(LangEx.FeatureString("WaypointManager.Dialogue.Imports", "NumberOfWaypoints", 
                    dto.Count.ToString("N0"), LangEx.Pluralise(waypointCode, dto.Count)));
                list.Add(new WaypointImportCellEntry
                {
                    Title = dto.Name,
                    DetailText = detailText.ToString(),
                    Enabled = true,
                    RightTopText = dto.World,
                    RightTopOffY = 3f,
                    DetailTextFont = CairoFont.WhiteDetailText().WithFontSize((float)GuiStyle.SmallFontSize),
                    Model = dto
                });
            }
            catch (Exception exception)
            {
                G.Logger.Error("Error caught while loading waypoints from import file.");
                G.Logger.Error(exception);
                return [];
            }
        }
        return list;
    }

    /// <summary>
    ///     Refreshes the displayed values on the form.
    /// </summary>
    protected override void RefreshValues()
    {
        if (SingleComposer is null) return;

        ApiEx.ClientMain.EnqueueMainThreadTask(() =>
        {
            var cellList = _filesList.elementCells.Cast<WaypointImportGuiCell>().Where(p => p.On).ToList();
            var totalCount = cellList.Sum(p => p.Cell.Model is not null && p.Cell.Model.Waypoints is not null ? p.Cell.Model.Waypoints.Count : 0);
                
            var code = LangEx.FeatureCode("WaypointManager.Dialogue.Imports", "File");
            var pluralisedFile = LangEx.Pluralise(code, cellList.Count);
                
            var labelText = LangEx.FeatureString("WaypointManager.Dialogue.Imports", "SelectedWaypoints", 
                totalCount.ToString("N0"), cellList.Count.ToString("N0"), pluralisedFile);

            _cellListBounds.CalcWorldBounds();
            _clippedBounds.CalcWorldBounds();
            _lblSelectedCount.SetNewText(labelText, true);
            SingleComposer.GetScrollbar("scrollbar").SetHeights((float)_clippedBounds.fixedHeight, (float)_cellListBounds.fixedHeight);
        }, "");
    }

    /// <summary>
    ///     Composes the main body of the dialogue window.
    /// </summary>
    /// <param name="composer">The GUI composer.</param>
    protected override void ComposeBody(GuiComposer composer)
    {
        var platform = capi.World.GetField<ClientPlatformAbstract>("Platform");
        var scaledWidth = Math.Max(600, platform.WindowSize.Width * 0.5) / ClientSettings.GUIScale;
        var scaledHeight = Math.Max(600, (platform.WindowSize.Height - 65) * 0.85) / ClientSettings.GUIScale;

        var buttonRowBoundsRightFixed = ElementBounds
            .FixedSize(60, 30)
            .WithFixedPadding(10, 2)
            .WithAlignment(EnumDialogArea.RightFixed);

        var buttonRowBounds = ElementBounds
            .FixedSize(60, 30)
            .WithFixedPadding(10, 2);

        var textBounds = ElementBounds
            .FixedSize(300, 30)
            .WithFixedPadding(10, 2)
            .WithAlignment(EnumDialogArea.CenterTop);

        var outerBounds = ElementBounds
            .Fixed(EnumDialogArea.LeftTop, 0, 0, scaledWidth, 35);

        var insetBounds = outerBounds
            .BelowCopy(0, 3)
            .WithFixedSize(scaledWidth, scaledHeight);

        _clippedBounds = insetBounds
            .ForkContainingChild(3, 3, 3, 3);

        _cellListBounds = _clippedBounds
            .ForkContainingChild(0.0, 0.0, 0.0, -3.0)
            .WithFixedPadding(10.0);

        _filesList = new GuiElementCellList<WaypointImportCellEntry>(capi, _cellListBounds, OnRequireCell, _files);

        _lblSelectedCount =
            new GuiElementDynamicText(capi, string.Empty,
                CairoFont.WhiteDetailText().WithOrientation(EnumTextOrientation.Center),
                textBounds.FixedUnder(insetBounds, 10));

        composer
            .AddInset(insetBounds)
            .AddVerticalScrollbar(OnScroll, ElementStdBounds.VerticalScrollbar(insetBounds), "scrollbar")
            .BeginClip(_clippedBounds)
            .AddInteractiveElement(_filesList)
            .EndClip()
                
            .AddSmallButton(LangEx.FeatureString("WaypointManager.Dialogue.Exports", "OpenExportsFolder"), OnOpenImportsFolderButtonPressed,
                buttonRowBounds.FlatCopy().FixedUnder(insetBounds, 10.0))

            .AddInteractiveElement(_lblSelectedCount)
                
            .AddSmallButton(LangEx.FeatureString("WaypointManager.Dialogue.Imports", "ImportSelectedWaypoints"), OnImportSelectedWaypointsButtonPressed,
                buttonRowBoundsRightFixed.FlatCopy().FixedUnder(insetBounds, 10.0));
    }

    #endregion

    #region Control Event Handlers

    /// <summary>
    ///     Called when the GUI needs to refresh or create a cell to display to the user. 
    /// </summary>
    private WaypointImportGuiCell OnRequireCell(WaypointImportCellEntry cell, ElementBounds bounds)
    {
        return new WaypointImportGuiCell(ApiEx.Client, cell, bounds)
        {
            On = false,
            OnMouseDownOnCellLeft = OnCellClick,
            OnMouseDownOnCellRight = OnCellClick
        };
    }

    private void OnScroll(float dy)
    {
        var bounds = _filesList.Bounds;
        bounds.fixedY = 0f - dy;
        bounds.CalcWorldBounds();
    }

    /// <summary>
    ///     Called when the user clicks on one of the cells in the grid.
    /// </summary>
    private void OnCellClick(int val)
    {
        var cell = _filesList.elementCells.Cast<WaypointImportGuiCell>().ToList()[val];
        cell.On = !cell.On;
        cell.Enabled = cell.On;
        RefreshValues();
    }

    /// <summary>
    ///     Called when the user adds, removes, or renames files within the imports folder.
    /// </summary>
    private void OnDirectoryChanged(object sender, FileSystemEventArgs e)
    {
        if (e.ChangeType is WatcherChangeTypes.Changed) return;
        if (++_watcherLock > 1) return;
        _watcher.EnableRaisingEvents = false;
        RefreshFiles();
        RefreshValues();
    }

    /// <summary>
    ///     Called when the user presses the "Open Imports Folder" button.
    /// </summary>
    private bool OnOpenImportsFolderButtonPressed()
    {
        NetUtil.OpenUrlInBrowser(_importsDirectory);
        return true;
    }

    /// <summary>
    ///     Called when the user presses the "Import" button.
    /// </summary>
    private bool OnImportSelectedWaypointsButtonPressed()
    {
        var files = _filesList.elementCells
            .Cast<WaypointImportGuiCell>()
            .Where(p => p.On)
            .Select(w => w.Cell)
            .ToList();

        if (files.Count == 0) return false;

        var list = new List<ImportedWaypointTemplate>();
        foreach (var file in files)
        {
            var waypoints = file.Model?.Waypoints
                .Select(p => new ImportedWaypointTemplate(p, file.Model.SpawnPosition));
            if (waypoints is null || !waypoints.Any()) continue;
            list.AddRange(waypoints);
        }
        
        var dialogue = new WaypointImportConfirmationDialogue(capi, list);
        dialogue.TryOpen();
        return TryClose();
    }

    #endregion

    /// <summary>
    ///     Disposes the dialogue window.
    /// </summary>
    public override void Dispose()
    {
        GC.SuppressFinalize(this);
        _watcher.Dispose();
        _filesList.Dispose();
        _lblSelectedCount.Dispose();
        base.Dispose();
    }
}