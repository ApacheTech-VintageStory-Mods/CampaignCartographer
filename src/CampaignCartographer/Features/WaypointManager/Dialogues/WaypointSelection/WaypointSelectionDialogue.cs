using ApacheTech.VintageMods.CampaignCartographer.Features.WaypointManager.Extensions;
using ApacheTech.VintageMods.CampaignCartographer.Features.WaypointManager.Models;
using ApacheTech.VintageMods.CampaignCartographer.Features.WaypointManager.WaypointTemplates;
using Gantry.Core.GameContent.GUI.Abstractions;

namespace ApacheTech.VintageMods.CampaignCartographer.Features.WaypointManager.Dialogues.WaypointSelection;

public abstract class WaypointSelectionDialogue : GenericDialogue
{
    private GuiElementDynamicText? _lblSelectedCount;

    protected WaypointSelectionDialogue(ICoreClientAPI capi) : base(capi)
    {
        Alignment = EnumDialogArea.CenterMiddle;
    }

    #region Form Composition

    protected WaypointSelectionDialogueBounds Bounds { get; } = new();

    protected override void PreCompose() => RefreshCells();

    protected override void ComposeBody(GuiComposer composer)
    {
        AddHeaderRow(composer);
        AddCellListPanel(composer);
        AddFooterRow(composer);
    }

    protected override void RefreshValues()
    {
        base.RefreshValues();
        if (SingleComposer is null) return;
        SingleComposer.GetTextInput("txtSearchBox").SetValue(SearchTerm);

        var cellList = CellList?.elementCells.Cast<WaypointSelectionGuiCell>();
        var count = cellList?.Count(p => p.On);
        var selectedCountText = LangEx.FeatureString("WaypointManager.Dialogue.Exports", "SelectedWaypoints", count?.ToString("N0"));
        _lblSelectedCount?.SetNewText(selectedCountText, true, true);
        UpdateCellListBounds();
    }

    protected void RecomposeBody()
    {
        PreCompose();
        Compose();
        RefreshValues();
    }

    #endregion

    #region Header

    protected virtual string HeaderButtonText { get; set; } = string.Empty;

    protected WaypointSortType SortOrder { get; private set; } = WaypointSortType.IndexAscending;

    protected virtual ActionConsumable HeaderButtonAction { get; set; } = null!;

    private void AddHeaderRow(GuiComposer composer)
    {
        var font = CairoFont.WhiteSmallText();
        var lblSearchText = $"{Lang.Get("Search")}...";

        var first = ElementBounds.Fixed(0, 0, 200, Bounds.SwitchSize).FixedUnder(Bounds.HeaderBounds, 3);
        var second = ElementBounds.FixedSize(250, Bounds.SwitchSize).FixedUnder(Bounds.HeaderBounds, 3).FixedRightOf(first, 10);

        var txtSearchBox = new GuiElementTextInput(ApiEx.Client, first, OnFilterTextChanged, CairoFont.TextInput());
        txtSearchBox.SetPlaceHolderText(lblSearchText);
        composer.AddInteractiveElement(txtSearchBox, "txtSearchBox");

        var keys = Enum.GetNames<WaypointSortType>();
        var values = keys.Select(p => T(p)).ToArray();

        var selectedIndex = SortOrder.To<int>();
        var cbxSortType = new GuiElementDropDown(ApiEx.Client, keys, values, selectedIndex, OnSortOrderChanged, second, font, false);
        composer.AddInteractiveElement(cbxSortType, "cbxSortType");

        var lblSelectedCountBounds = ElementBounds.FixedSize(250, 0).WithFixedPadding(10, 2).FixedUnder(Bounds.HeaderBounds, 10).FixedRightOf(second, 10);

        _lblSelectedCount = new GuiElementDynamicText(capi, "", CairoFont.WhiteDetailText().WithOrientation(EnumTextOrientation.Left), lblSelectedCountBounds);
        composer.AddInteractiveElement(_lblSelectedCount, "lblSelectedCount");

        if (HeaderButtonAction is not null && !string.IsNullOrEmpty(HeaderButtonText))
        {
            composer.AddSmallButton(
                text: HeaderButtonText,
                onClick: HeaderButtonAction,
                bounds: ElementBounds.FixedSize(60, 30).WithFixedPadding(10, 2).WithAlignment(EnumDialogArea.RightFixed).FixedUnder(Bounds.HeaderBounds),
                style: EnumButtonStyle.Normal,
                key: "btnHeader");
        }
    }

    private void OnFilterTextChanged(string filterString)
    {
        if (SearchTerm == filterString) return;
        SearchTerm = filterString;
        RecomposeBody();
    }

    private void OnSortOrderChanged(string code, bool selected)
    {
        var parsed = Enum.TryParse(code, false, out WaypointSortType type);
        if (!parsed || type == SortOrder) return;
        SortOrder = type;
        RecomposeBody();
    }

    #endregion

    #region Content

    private void AddCellListPanel(GuiComposer composer)
    {
        composer
            .AddInset(Bounds.InsetBounds)
            .AddVerticalScrollbar(OnScroll, ElementStdBounds.VerticalScrollbar(Bounds.InsetBounds), "scrollbar")
            .BeginClip(Bounds.ClippedBounds)
            .AddInteractiveElement(CellList, "cellList")
            .EndClip();
    }

    private void UpdateCellListBounds()
    {
        Bounds.CellListBounds.CalcWorldBounds();
        Bounds.ClippedBounds.CalcWorldBounds();
        SingleComposer.GetScrollbar("scrollbar").SetHeights((float)Bounds.ClippedBounds.fixedHeight, (float)Bounds.CellListBounds.fixedHeight);
    }

    private void OnScroll(float dy)
    {
        var bounds = SingleComposer.GetElement("cellList").Bounds;
        bounds.fixedY = 0f - dy;
        bounds.CalcWorldBounds();
    }

    #endregion

    #region Cell List Management

    protected abstract IEnumerable<WaypointSelectionCellEntry> GetCellEntries(System.Func<SelectableWaypointTemplate, bool> filter);

    protected IEnumerable<WaypointSelectionCellEntry> Cells { get; private set; } = [];

    protected GuiElementCellList<WaypointSelectionCellEntry>? CellList { get; private set; }

    protected virtual string SearchTerm { get; set; } = string.Empty;

    protected virtual void OnCellClickLeftSide(MouseEvent @event, int elementIndex)
    {
        var cell = CellList?.elementCells.Cast<WaypointSelectionGuiCell>().ToList()[elementIndex];
        if (cell is null) return;

        var worldMap = capi.ModLoader.GetModSystem<WorldMapManager>();
        worldMap.RecentreMap(cell.Model.Position);

        if (@event.Button != EnumMouseButton.Right) return;

        var dialogue = new AddEditWaypointDialogue(
            ApiEx.Client,
            cell.Model.ToWaypoint(),
            cell.CellEntry.Index);

        dialogue.OnClosed += RecomposeBody;
        dialogue.ToggleGui();
    }

    private void OnCellClickRightSide(MouseEvent @event, int elementIndex)
    {
        var cell = CellList?.elementCells.Cast<WaypointSelectionGuiCell>().ToList()[elementIndex];
        if (cell is null) return;

        cell.On = !cell.On;
        cell.Enabled = cell.On;
        cell.Model.Selected = cell.On;
        RefreshValues();
    }

    private void RefreshCells()
    {
        Cells = GetCellEntries(Filter);
        CellList = new GuiElementCellList<WaypointSelectionCellEntry>(capi, Bounds.CellListBounds, CellCreator, Cells);

        WaypointSelectionGuiCell CellCreator(WaypointSelectionCellEntry cell, ElementBounds bounds) => new(capi, cell, bounds)
        {
            On = cell.Model?.Selected ?? false,
            OnMouseDownOnCellLeft = OnCellClickLeftSide,
            OnMouseDownOnCellRight = OnCellClickRightSide
        };
    }

    private bool Filter(SelectableWaypointTemplate waypoint)
    {
        var filtered = string.IsNullOrWhiteSpace(SearchTerm) 
                    || waypoint.Title.Contains(SearchTerm, StringComparison.InvariantCultureIgnoreCase);
        waypoint.Selected = waypoint.Selected && filtered;
        return filtered;
    }

    #endregion

    #region Footer

    protected virtual string PrimaryButtonText { get; set; } = string.Empty;

    protected virtual string SecondaryButtonText { get; set; } = string.Empty;

    protected virtual string TertiaryButtonText { get; set; } = string.Empty;

    protected virtual ActionConsumable PrimaryButtonAction { get; set; } = null!;

    protected virtual ActionConsumable SecondaryButtonAction { get; set; } = null!;

    protected virtual ActionConsumable TertiaryButtonAction { get; set; } = null!;

    private void AddFooterRow(GuiComposer composer)
    {
        if (PrimaryButtonAction is not null && !string.IsNullOrEmpty(PrimaryButtonText))
        {
            composer.AddSmallButton(
                text: PrimaryButtonText,
                onClick: PrimaryButtonAction,
                bounds: Bounds.ButtonRowBoundsRightFixed.FlatCopy().FixedUnder(Bounds.InsetBounds, Bounds.GapBetweenRows),
                key: "btnPrimary");
        }

        if (SecondaryButtonAction is not null && !string.IsNullOrEmpty(SecondaryButtonText))
        {
            composer.AddSmallButton(
                text: SecondaryButtonText,
                onClick: SecondaryButtonAction,
                bounds: Bounds.ButtonRowBounds.FlatCopy().FixedUnder(Bounds.InsetBounds, Bounds.GapBetweenRows),
                style: EnumButtonStyle.Normal,
                key: "btnSecondary");
        }

        if (TertiaryButtonAction is not null && !string.IsNullOrEmpty(TertiaryButtonText))
        {
            composer.AddSmallButton(
                text: TertiaryButtonText,
                onClick: TertiaryButtonAction,
                bounds: Bounds.TextBounds.FlatCopy().FixedUnder(Bounds.InsetBounds, Bounds.GapBetweenRows),
                style: EnumButtonStyle.Normal,
                key: "btnTertiary");
        }
    }

    #endregion

    #region Utility Methods

    protected static string T(string text, params object[] args)
        => LangEx.FeatureString("WaypointManager.Dialogue", text, args);

    #endregion

    #region Base Overrides

    public override bool DisableMouseGrab => true;

    #endregion
}