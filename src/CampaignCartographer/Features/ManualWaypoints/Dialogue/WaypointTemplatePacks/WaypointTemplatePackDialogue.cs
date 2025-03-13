using ApacheTech.VintageMods.CampaignCartographer.Features.ManualWaypoints.Dialogue.PredefinedWaypoints;
using Gantry.Core.Extensions.DotNet;
using Gantry.Core.GameContent.GUI.Abstractions;
using Gantry.Services.FileSystem.Configuration;
using Vintagestory.Client;

namespace ApacheTech.VintageMods.CampaignCartographer.Features.ManualWaypoints.Dialogue.WaypointTemplatePacks;

/// <summary>
///     Dialogue Window: Allows the user to import waypoints from JSON files.
/// </summary>
/// <seealso cref="GenericDialogue" />
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public class WaypointTemplatePackDialogue : GenericDialogue
{
    private ElementBounds _clippedBounds;
    private ElementBounds _cellListBounds;

    private GuiElementCellList<WaypointTemplatePackCellEntry> _filesList;
    private GuiElementDynamicText _lblSelectedCount;
    private readonly PredefinedWaypointsSettings _settings;

    /// <summary>
    /// 	Initialises a new instance of the <see cref="WaypointImportDialogue" /> class.
    /// </summary>
    /// <param name="capi">Client API pass-through</param>
    public WaypointTemplatePackDialogue(ICoreClientAPI capi, PredefinedWaypointsSettings settings) : base(capi)
    {
        _settings = settings;
        Title = LangEx.FeatureString("PredefinedWaypoints.Dialogue", "Title");
        Alignment = EnumDialogArea.CenterMiddle;
    }

    #region Form Composition

    /// <summary>
    ///     Composes the GUI components for this instance.
    /// </summary>
    protected override void Compose()
    {
        base.Compose();
        RefreshTemplatePacks();
    }

    /// <summary>
    ///     Refreshes the file list, displayed on the form, whenever changes are made.
    /// </summary>
    private void RefreshTemplatePacks()
    {
        _filesList.ReloadCells(CreateCellEntries());
    }

    private List<WaypointTemplatePackCellEntry> CreateCellEntries()
    {
        var packs = Systems.PredefinedWaypoints.TemplatePacks;
        var list = new List<WaypointTemplatePackCellEntry>();
        foreach (var pack in packs)
        {
            try
            {
                var totalCount = pack.Templates.Count;
                var selectedCount = pack.Templates.Count - _settings.DisabledTemplatesPerPack.CountList(pack.Metadata.Name);
                var topRightText = $"{selectedCount}/{totalCount} {(pack.Templates.Count == 1 ? "template" : "templates")}";
                list.Add(new WaypointTemplatePackCellEntry
                {
                    Title = pack.Metadata.Title,
                    DetailText = pack.Metadata.Description,
                    Enabled = pack.Metadata.Enabled,
                    RightTopText = topRightText,
                    RightTopOffY = 3f,
                    DetailTextFont = CairoFont.WhiteDetailText().WithFontSize((float)GuiStyle.SmallFontSize),
                    Model = pack
                });
            }
            catch (Exception exception)
            {
                G.Log.Error(exception);
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
            var cellList = _filesList.elementCells.Cast<WaypointTemplatePackGuiCell>().Where(p => p.On).ToList();
            var totalSelectedCount = GetTotalSelectedCount(cellList);

            var code = LangEx.FeatureCode("WaypointManager.Dialogue.Imports", "File");
            var pluralisedFile = LangEx.Pluralise(code, cellList.Count);

            var labelText = LangEx.FeatureString("WaypointManager.Dialogue.Imports", "SelectedWaypoints",
                totalSelectedCount.ToString("N0"), cellList.Count.ToString("N0"), pluralisedFile);

            _cellListBounds.CalcWorldBounds();
            _clippedBounds.CalcWorldBounds();
            _lblSelectedCount.SetNewText(labelText, true);
            SingleComposer.GetScrollbar("scrollbar").SetHeights((float)_clippedBounds.fixedHeight, (float)_cellListBounds.fixedHeight);
        }, "");
    }

    private int GetTotalSelectedCount(List<WaypointTemplatePackGuiCell> cellList)
    {
        var totalCount = cellList.Sum(p => p.Cell.Model.Templates.Count);
        var selectedCount = cellList.Sum(p => _settings.DisabledTemplatesPerPack.CountList(p.Cell.Model.Metadata.Name));
        var totalSelectedCount = totalCount - selectedCount;
        return totalSelectedCount;
    }

    /// <summary>
    ///     Composes the main body of the dialogue window.
    /// </summary>
    /// <param name="composer">The GUI composer.</param>
    protected override void ComposeBody(GuiComposer composer)
    {
        var scaledWidth = Math.Max(600, ScreenManager.Platform.WindowSize.Width * 0.5) / ClientSettings.GUIScale;
        var scaledHeight = Math.Max(600, (ScreenManager.Platform.WindowSize.Height - 65) * 0.85) / ClientSettings.GUIScale;

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

        _filesList = new GuiElementCellList<WaypointTemplatePackCellEntry>(capi, _cellListBounds, OnRequireCell, CreateCellEntries());

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

            .AddInteractiveElement(_lblSelectedCount)

            .AddSmallButton(Lang.Get("Close"), TryClose, buttonRowBoundsRightFixed.FlatCopy().FixedUnder(insetBounds, 10.0));
    }

    #endregion

    #region Control Event Handlers

    /// <summary>
    ///     Called when the GUI needs to refresh or create a cell to display to the user. 
    /// </summary>
    private IGuiElementCell OnRequireCell(WaypointTemplatePackCellEntry cell, ElementBounds bounds)
    {
        return new WaypointTemplatePackGuiCell(ApiEx.Client, cell, bounds)
        {
            On = cell.Enabled = cell.Model.Metadata.Enabled,
            OnMouseDownOnCellLeft = OnCellClickLeftSide,
            OnMouseDownOnCellRight = OnCellClickRightSide
        };
    }

    private void OnScroll(float dy)
    {
        var bounds = _filesList.Bounds;
        bounds.fixedY = 0f - dy;
        bounds.CalcWorldBounds();
    }

    private void OnCellClickLeftSide(int val)
    {
        var cell = _filesList.elementCells.Cast<WaypointTemplatePackGuiCell>().ToList()[val];
        var dialogue = IOC.Services.CreateInstance<PredefinedWaypointsDialogue>(cell.Cell.Model).With(p =>
        {
            p.Title = cell.Cell.Model.Metadata.Title;
            p.OnClose = () =>
            {
                RefreshTemplatePacks(); 
                RefreshValues();
            };
        });
        dialogue.TryOpen();
    }

    /// <summary>
    ///     Called when the user clicks on one of the cells in the grid.
    /// </summary>
    private void OnCellClickRightSide(int val)
    {
        var cell = _filesList.elementCells.Cast<WaypointTemplatePackGuiCell>().ToList()[val];
        cell.Cell.Model.Metadata.Enabled = cell.On = cell.Enabled = !cell.On;
        if (cell.On)
        {
            if (_settings.DisabledTemplatePacks.RemoveIfPresent(cell.Cell.Model.Metadata.Name))
            {
                ModSettings.World.Save(_settings);
            }
        }
        else
        {
            if (_settings.DisabledTemplatePacks.AddIfNotPresent(cell.Cell.Model.Metadata.Name))
            {
                ModSettings.World.Save(_settings);
            }
        }
        RefreshValues();
    }

    #endregion

    /// <summary>
    ///     Disposes the dialogue window.
    /// </summary>
    public override void Dispose()
    {
        GC.SuppressFinalize(this);
        _filesList.Dispose();
        _lblSelectedCount.Dispose();
        base.Dispose();
    }
}