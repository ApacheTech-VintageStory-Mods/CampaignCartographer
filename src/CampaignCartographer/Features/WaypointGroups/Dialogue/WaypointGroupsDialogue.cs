using Gantry.Core.GameContent.GUI.Abstractions;

namespace ApacheTech.VintageMods.CampaignCartographer.Features.WaypointGroups.Dialogue;

/// <summary>
///     Dialogue Window: Allows the user to import waypoints from JSON files.
/// </summary>
/// <seealso cref="GenericDialogue" />
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public class WaypointGroupsDialogue : GenericDialogue
{
    private ElementBounds _clippedBounds;
    private ElementBounds _cellListBounds;

    private GuiElementCellList<WaypointGroupCellEntry> _groupCellList;
    private readonly WaypointGroupsSettings _settings;

    /// <summary>
    /// 	Initialises a new instance of the <see cref="WaypointImportDialogue" /> class.
    /// </summary>
    /// <param name="capi">Client API pass-through</param>
    public WaypointGroupsDialogue(ICoreClientAPI capi, WaypointGroupsSettings settings) : base(capi)
    {
        _settings = settings;
        Title = Systems.WaypointGroups.T("Dialogue.Title");
        Alignment = EnumDialogArea.CenterMiddle;
    }

    #region Form Composition

    /// <summary>
    ///     Composes the GUI components for this instance.
    /// </summary>
    protected override void Compose()
    {
        base.Compose();
        RefreshWaypointGroups();
    }

    /// <summary>
    ///     Recomposes the GUI components for this instance.
    /// </summary>
    private void Recompose()
    {
        Compose();
        RefreshValues();
    }

    /// <summary>
    ///     Refreshes the file list, displayed on the form, whenever changes are made.
    /// </summary>
    private void RefreshWaypointGroups()
    {
        _groupCellList.ReloadCells(CreateCellEntries());
    }

    private List<WaypointGroupCellEntry> CreateCellEntries()
    {
        var list = new List<WaypointGroupCellEntry>();
        foreach (var group in _settings.Groups)
        {
            try
            {
                list.Add(new WaypointGroupCellEntry
                {
                    Title = group.Title,
                    Enabled = true,
                    Model = group
                });
            }
            catch (Exception exception)
            {
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
            _cellListBounds.CalcWorldBounds();
            _clippedBounds.CalcWorldBounds();
            SingleComposer.GetScrollbar("scrollbar").SetHeights((float)_clippedBounds.fixedHeight, (float)_cellListBounds.fixedHeight);
        }, "");
    }

    /// <summary>
    ///     Composes the main body of the dialogue window.
    /// </summary>
    /// <param name="composer">The GUI composer.</param>
    protected override void ComposeBody(GuiComposer composer)
    {
        var scaledWidth = 400 / ClientSettings.GUIScale;
        var scaledHeight = 300 / ClientSettings.GUIScale;

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

        _groupCellList = new GuiElementCellList<WaypointGroupCellEntry>(capi, _cellListBounds, OnRequireCell, CreateCellEntries());

        composer
            .AddInset(insetBounds)
            .AddVerticalScrollbar(OnScroll, ElementStdBounds.VerticalScrollbar(insetBounds), "scrollbar")
            .BeginClip(_clippedBounds)
            .AddInteractiveElement(_groupCellList)
            .EndClip()

            .AddSmallButton(Lang.Get("Close"), TryClose, buttonRowBoundsRightFixed.FlatCopy().FixedUnder(insetBounds, 10.0))
            .AddSmallButton(Systems.WaypointGroups.T("Dialogue.AddNew"), OnAddNewGroupButtonClicked, buttonRowBoundsRightFixed.FlatCopy().FixedUnder(insetBounds, 10.0).FixedLeftOf(buttonRowBoundsRightFixed, 30.0));
    }

    #endregion

    #region Control Event Handlers

    private bool OnAddNewGroupButtonClicked()
    {
        var dialogue = new AddEditWaypointGroupDialogue 
        { 
            OnChanged = RefreshWaypointGroups
        };
        dialogue.ToggleGui();
        return true;
    }

    private IGuiElementCell OnRequireCell(WaypointGroupCellEntry cell, ElementBounds bounds)
    {
        return new WaypointGroupGuiCell(ApiEx.Client, cell, bounds)
        {
            OnMouseDownOnCell = OnCellClick,
        };
    }

    private void OnScroll(float dy)
    {
        var bounds = _groupCellList.Bounds;
        bounds.fixedY = 0f - dy;
        bounds.CalcWorldBounds();
    }

    private void OnCellClick(int val)
    {
        var group = _groupCellList.elementCells.Cast<WaypointGroupGuiCell>().ToList()[val].Cell.Model;
        var dialogue = new AddEditWaypointGroupDialogue(group) { OnChanged = RefreshWaypointGroups };
        dialogue.ToggleGui();
    }

    #endregion

    /// <summary>
    ///     Disposes the dialogue window.
    /// </summary>
    public override void Dispose()
    {
        GC.SuppressFinalize(this);
        _groupCellList.Dispose();
        base.Dispose();
    }
}