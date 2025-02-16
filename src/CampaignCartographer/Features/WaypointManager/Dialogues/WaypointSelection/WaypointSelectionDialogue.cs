using ApacheTech.Common.Extensions.Harmony;
using ApacheTech.VintageMods.CampaignCartographer.Features.WaypointManager.Dialogues.Imports;
using ApacheTech.VintageMods.CampaignCartographer.Features.WaypointManager.Extensions;
using ApacheTech.VintageMods.CampaignCartographer.Features.WaypointManager.Models;
using ApacheTech.VintageMods.CampaignCartographer.Features.WaypointManager.Repositories;
using ApacheTech.VintageMods.CampaignCartographer.Features.WaypointManager.WaypointTemplates;
using Gantry.Core.Extensions.DotNet;
using Gantry.Core.GameContent.GUI.Abstractions;
using Vintagestory.API.Util;

namespace ApacheTech.VintageMods.CampaignCartographer.Features.WaypointManager.Dialogues.WaypointSelection;

/// <summary>
///     Dialogue Window: Allows the user to export waypoints to JSON files.
/// </summary>
/// <seealso cref="GenericDialogue" />
[HarmonyClientSidePatch]
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public abstract class WaypointSelectionDialogue : GenericDialogue
{
    private ElementBounds _clippedBounds;
    private ElementBounds _cellListBounds;
    private GuiElementDynamicText _lblSelectedCount;
    private static WaypointSelectionDialogue _instance;
    private string _filterString;
    private readonly IPlayer[] _onlinePlayers;
    protected readonly WorldMapManager WorldMap;

    protected List<WaypointSelectionCellEntry> Waypoints { get; set; } = [];

    protected GuiElementCellList<WaypointSelectionCellEntry> WaypointsList { get; private set; }

    protected WaypointSortType SortOrder { get; private set; } = WaypointSortType.IndexAscending;

    protected bool ShowExtraButtons { private get; init; }

    protected string LeftButtonText { private get; init; }

    protected string RightButtonText { private get; init; }

    public override bool DisableMouseGrab => true;

    /// <summary>
    /// 	Initialises a new instance of the <see cref="WaypointSelectionDialogue" /> class.
    /// </summary>
    /// <param name="capi">Client API pass-through</param>
    /// <param name="queriesRepo">IOC Injected Waypoint Service.</param>
    protected WaypointSelectionDialogue(ICoreClientAPI capi, WaypointQueriesRepository queriesRepo) : base(capi)
    {
        WorldMap = IOC.Services.Resolve<WorldMapManager>();
        Title = LangEx.FeatureString("WaypointManager.Dialogue.Exports", "Title");
        Alignment = EnumDialogArea.CenterMiddle;
        _onlinePlayers = capi.World.AllOnlinePlayers.Except([capi.World.Player]).ToArray();

        ClientSettings.Inst.AddWatcher<float>("guiScale", _ =>
        {
            Compose();
            RefreshValues();
        });
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(WaypointMapLayer), "OnDataFromServer")]
    public static void Patch_WaypointMapLayer_OnDataFromServer_PostFix(byte[] data)
    {
        _instance?.RefreshWaypoints(data);
        _instance?.SingleComposer.ReCompose();
    }

    #region Form Composition

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

    protected override void Compose()
    {
        base.Compose();
        PopulateCellList();
    }

    protected virtual void PopulateCellList()
    {
        WaypointsList.ReloadCells(Waypoints);
    }

    protected override void RefreshValues()
    {
        if (SingleComposer is null) return;

        var cellList = WaypointsList.elementCells.Cast<WaypointSelectionGuiCell>();
        var count = cellList.Count(p => p.On);
        var selectedCountText = LangEx.FeatureString("WaypointManager.Dialogue.Exports", "SelectedWaypoints", count.ToString("N0"));
        _lblSelectedCount.SetNewText(selectedCountText, true, true);

        _cellListBounds.CalcWorldBounds();
        _clippedBounds.CalcWorldBounds();
        SingleComposer.GetScrollbar("scrollbar").SetHeights((float)_clippedBounds.fixedHeight, (float)_cellListBounds.fixedHeight);
    }

    protected override void ComposeBody(GuiComposer composer)
    {
        var platform = capi.World.GetField<ClientPlatformAbstract>("Platform");
        var scaledWidth = Math.Max(600, platform.WindowSize.Width * 0.5) / ClientSettings.GUIScale;
        var scaledHeight = Math.Min(600, (platform.WindowSize.Height - 65) * 0.85) / ClientSettings.GUIScale;

        var buttonRowBoundsRightFixed = ElementBounds
            .FixedSize(60, 30)
            .WithFixedPadding(10, 2)
            .WithAlignment(EnumDialogArea.RightFixed);

        var buttonRowBounds = ElementBounds
            .FixedSize(60, 30)
            .WithFixedPadding(10, 2);

        var textBounds = ElementBounds
            .FixedSize(EnumDialogArea.CenterFixed, 0, 30)
            .WithFixedPadding(10, 2);

        var outerBounds = ElementBounds
            .Fixed(EnumDialogArea.LeftTop, 0, 0, scaledWidth, 35);

        AddSearchBox(composer, ref outerBounds);

        var insetBounds = outerBounds
            .BelowCopy(0, 3)
            .WithFixedSize(scaledWidth, scaledHeight);

        _clippedBounds = insetBounds
            .ForkContainingChild(3, 3, 3, 3);

        _cellListBounds = _clippedBounds
            .ForkContainingChild(0.0, 0.0, 0.0, -3.0)
            .WithFixedPadding(10.0);

        WaypointsList = new GuiElementCellList<WaypointSelectionCellEntry>(capi, _cellListBounds, CellCreator, Waypoints);

        composer
            .AddInset(insetBounds)
            .AddVerticalScrollbar(OnScroll, ElementStdBounds.VerticalScrollbar(insetBounds), "scrollbar")
            .BeginClip(_clippedBounds)
            .AddInteractiveElement(WaypointsList, "waypointsList")
            .EndClip();

        composer.AddSmallButton(LeftButtonText, OnLeftButtonPressed,
            buttonRowBounds.FlatCopy().FixedUnder(insetBounds, 10.0),
            EnumButtonStyle.Normal, "btnOpenExportsFolder");

        if (ShowExtraButtons && _onlinePlayers.Length > 0)
        {
            composer.AddSmallButton(T("ShareSelected"),OnShareButtonPressed, 
                textBounds.FlatCopy().FixedUnder(insetBounds, 10.0), EnumButtonStyle.Normal, "btnShareSelected");
        }

        composer.AddSmallButton(RightButtonText, OnRightButtonPressed, buttonRowBoundsRightFixed.FlatCopy().FixedUnder(insetBounds, 10.0));
    }

    private void AddSearchBox(GuiComposer composer, ref ElementBounds bounds)
    {
        const int switchSize = 30;
        const int gapBetweenRows = 20;
        var font = CairoFont.WhiteSmallText();
        var lblSearchText = $"{Lang.Get("Search")}...";

        var first = ElementBounds.Fixed(0, 0, 200, switchSize).FixedUnder(bounds, 3);
        var second = ElementBounds.FixedSize(250, switchSize).FixedUnder(bounds, 3).FixedRightOf(first, 10);

        var txtSearchBox = new GuiElementTextInput(ApiEx.Client, first, OnFilterTextChanged, CairoFont.TextInput());
        txtSearchBox.SetPlaceHolderText(lblSearchText);
        composer.AddInteractiveElement(txtSearchBox, "txtSearchBox");

        var keys = Enum.GetNames(typeof(WaypointSortType));
        var values = keys.Select(p => T(p)).ToArray();

        var cbxSortType = new GuiElementDropDown(ApiEx.Client, keys, values, 0, OnSortOrderChanged, second, font, false);
        composer.AddInteractiveElement(cbxSortType, "cbxSortType");

        var btnShareBounds = ElementBounds.FixedSize(250, 0).WithFixedPadding(10, 2).FixedUnder(bounds, 10).FixedRightOf(second, 10);
        _lblSelectedCount = new GuiElementDynamicText(capi, "", CairoFont.WhiteDetailText().WithOrientation(EnumTextOrientation.Left), btnShareBounds);
        composer.AddInteractiveElement(_lblSelectedCount, "lblSelectedCount");

        if (ShowExtraButtons)
        {
            var btnOpenImportsBounds = ElementBounds
                .FixedSize(60, 30)
                .WithFixedPadding(10, 2)
                .WithAlignment(EnumDialogArea.RightFixed)
                .FixedUnder(bounds);

            composer.AddSmallButton(LangEx.FeatureString("WaypointManager.Dialogue.Imports", "Title"),
                OnImportButtonClicked, btnOpenImportsBounds, EnumButtonStyle.Normal, "btnOpenImports");
        }

        bounds = bounds.BelowCopy(fixedDeltaY: gapBetweenRows);
    }

    private void OnSortOrderChanged(string code, bool selected)
    {
        if (Enum.TryParse(code, false, out WaypointSortType type)) SortOrder = type;
        PopulateCellList();
        FilterCells();
        RefreshValues();
    }

    private bool OnShareButtonPressed()
    {
        var cellList = WaypointsList.elementCells.Cast<WaypointSelectionGuiCell>().Where(p => p.On);        
        var waypoints = cellList.Select(p => p.Model.ToWaypoint());
        var dialogue = new ShareWaypointDialogue(capi, _onlinePlayers, waypoints);
        dialogue.ToggleGui();
        return true;
    }

    private bool OnImportButtonClicked()
    {
        var dialogue = IOC.Services.Resolve<WaypointImportDialogue>();
        dialogue.OnClosed += () => TryClose();
        return dialogue.TryOpen();
    }

    protected static string T(string text, params object[] args) 
        => LangEx.FeatureString("WaypointManager.Dialogue.Exports", text, args);

    private void OnFilterTextChanged(string filterString)
    {
        _filterString = filterString;
        FilterCells();
        RefreshValues();
    }

    private void FilterCells()
    {
        WaypointsList.CallMethod("FilterCells", (System.Func<IGuiElementCell, bool>)Filter);
        return;

        bool Filter(IGuiElementCell cell)
        {
            var c = (WaypointSelectionGuiCell)cell;
            var model = c.Model;
            var state = string.IsNullOrWhiteSpace(_filterString) ||
                        model.Title.ToLowerInvariant().Contains(_filterString.ToLowerInvariant());
            return state;
        }
    }

    #endregion

    #region Cell List Management

    private IGuiElementCell CellCreator(WaypointSelectionCellEntry cell, ElementBounds bounds)
    {
        return new WaypointSelectionGuiCell(ApiEx.Client, cell, bounds)
        {
            On = cell.Model.Selected,
            OnMouseDownOnCellLeft = OnCellClickLeftSide,
            OnMouseDownOnCellRight = OnCellClickRightSide
        };
    }

    #endregion

    #region Control Event Handlers

    private void OnScroll(float dy)
    {
        var bounds = SingleComposer.GetElement("waypointsList").Bounds;
        bounds.fixedY = 0f - dy;
        bounds.CalcWorldBounds();
    }

    protected virtual void OnCellClickLeftSide(MouseEvent args, int elementIndex)
    {
        var cell = WaypointsList.elementCells.Cast<WaypointSelectionGuiCell>().ToList()[elementIndex];
        WorldMap.RecentreMap(cell.Model.Position);
        if (args.Button != EnumMouseButton.Right) return;

        var dialogue = new AddEditWaypointDialogue(
            ApiEx.Client, 
            cell.Model.ToWaypoint(),
            cell.CellEntry.Index);

        dialogue.OnClosed += RefreshWaypoints;
        dialogue.ToggleGui();
    }

    private void RefreshWaypoints()
    {
        PopulateCellList();
        RefreshValues();
    }

    private void RefreshWaypoints(byte[] data)
    {
        var waypoints = SerializerUtil.Deserialize<List<Waypoint>>(data);
        Waypoints = RefreshWaypointCellEntries(waypoints);
        WaypointsList.ReloadCells(Waypoints);
        RefreshValues();
    }

    private List<WaypointSelectionCellEntry> RefreshWaypointCellEntries(List<Waypoint> ownWaypoints)
    {
        var sortedWaypoints = ownWaypoints.ToSortedDictionary();
        var waypoints = WaypointQueriesRepository.Sort(SortOrder, sortedWaypoints)
            .Select(x => new KeyValuePair<int, SelectableWaypointTemplate>(x.Key, SelectableWaypointTemplate.FromWaypoint(x.Value)));
        var playerPos = ApiEx.Client.World.Player.Entity.Pos.AsBlockPos;

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

    protected void OnCellClickRightSide(MouseEvent args, int elementIndex)
    {
        var cell = WaypointsList.elementCells.Cast<WaypointSelectionGuiCell>().ToList()[elementIndex];
        cell.On = !cell.On;
        cell.Enabled = cell.On;
        cell.Model.Selected = cell.On;
        RefreshValues();
    }

    protected virtual bool OnLeftButtonPressed()
    {
        return true;
    }

    protected virtual bool OnRightButtonPressed()
    {
        return true;
    }

    #endregion
}