using ApacheTech.Common.Extensions.Harmony;
using ApacheTech.VintageMods.CampaignCartographer.Features.ManualWaypoints.Model;
using ApacheTech.VintageMods.CampaignCartographer.Features.WaypointManager.WaypointTemplates;
using Gantry.Core.Extensions.DotNet;
using Gantry.Core.GameContent.GUI.Abstractions;
using Gantry.Core.GameContent.GUI.Models;
using Gantry.Services.FileSystem.Abstractions.Contracts;
using Gantry.Services.FileSystem.Configuration;

namespace ApacheTech.VintageMods.CampaignCartographer.Features.ManualWaypoints.Dialogue.PredefinedWaypoints;

/// <summary>
///     GUI window that enables players to manage pre-defined waypoint types that can be added to the world map, through chat.
/// </summary>
/// <seealso cref="GenericDialogue" />
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public class PredefinedWaypointsDialogue : GenericDialogue
{
    private readonly IFileSystemService _fileSystemService;
    private readonly TemplatePack _templatePack;
    private readonly PredefinedWaypointsSettings _settings;

    private List<PredefinedWaypointsCellEntry> _waypointCells;
    private readonly SortedDictionary<string, PredefinedWaypointTemplate> _templates = [];
    private ElementBounds _clippedBounds;
    private ElementBounds _cellListBounds;
    private GuiElementCellList<PredefinedWaypointsCellEntry> _cellList;
    private string _filterString;
    private bool _disabledTypesOnly;

    /// <summary>
    /// 	Initialises a new instance of the <see cref="PredefinedWaypointsDialogue" /> class.
    /// </summary>
    /// <param name="capi">Client API pass-through</param>
    /// <param name="fileSystemService">Injected file system service.</param>
    public PredefinedWaypointsDialogue(ICoreClientAPI capi, IFileSystemService fileSystemService, PredefinedWaypointsSettings settings, TemplatePack templatePack) : base(capi)
    {
        _fileSystemService = fileSystemService;
        _settings = settings;
        _templatePack = templatePack;
        _templates.AddOrUpdateRange(templatePack.GetTemplates(), w => w.Key);
        _waypointCells = GetCellEntries();
        Title = LangEntry("Title");
        Alignment = EnumDialogArea.CenterMiddle;
        Modal = true;
        ModalTransparency = 0.4f;
    }

    private static string LangEntry(string text, params object[] args)
    {
        return LangEx.FeatureString("PredefinedWaypoints.Dialogue.PreDefinedWaypoints", text, args);
    }

    #region Form Composition

    protected override void Compose()
    {
        base.Compose();
        var cellList = SingleComposer.GetCellList<PredefinedWaypointsCellEntry>("waypointsList");
        cellList.ReloadCells(_waypointCells);
    }

    protected override void RefreshValues()
    {
        if (SingleComposer is null) return;
        _cellListBounds.CalcWorldBounds();
        _clippedBounds.CalcWorldBounds();
        SingleComposer.GetScrollbar("scrollbar").SetHeights((float)_clippedBounds.fixedHeight, (float)_cellListBounds.fixedHeight);
    }

    protected override void ComposeBody(GuiComposer composer)
    {
        var platform = capi.World.GetField<ClientPlatformAbstract>("Platform");
        var scaledWidth = Math.Max(600, platform.WindowSize.Width * 0.5) / ClientSettings.GUIScale;
        var scaledHeight = Math.Min(600, (platform.WindowSize.Height - 65) * 0.85) / ClientSettings.GUIScale;

        var outerBounds = ElementBounds
            .Fixed(EnumDialogArea.LeftTop, 0, 0, scaledWidth, 35);

        AddSearchBox(composer, ref outerBounds);

        var insetBounds = outerBounds
            .BelowCopy(0, 3)
            .WithFixedSize(scaledWidth, scaledHeight);

        var buttonRowBoundsLeftFixed = ElementBounds
            .FixedSize(150, 30)
            .WithFixedPadding(10, 2)
            .WithAlignment(EnumDialogArea.LeftFixed)
            .FixedUnder(insetBounds, 10.0);

        var buttonRowBoundsRightFixed = ElementBounds
            .FixedSize(150, 30)
            .WithFixedPadding(10, 2)
            .WithAlignment(EnumDialogArea.RightFixed)
            .FixedUnder(insetBounds, 10.0);

        _clippedBounds = insetBounds
            .ForkContainingChild(3, 3, 3, 3);

        _cellListBounds = _clippedBounds
            .ForkContainingChild(0.0, 0.0, 0.0, -3.0)
            .WithFixedPadding(10.0);

        _cellList = new GuiElementCellList<PredefinedWaypointsCellEntry>(capi, _cellListBounds, CellCreator, _waypointCells);

        composer
            .AddInset(insetBounds)
            .AddVerticalScrollbar(OnScroll, ElementStdBounds.VerticalScrollbar(insetBounds), "scrollbar")
            .BeginClip(_clippedBounds)
            .AddInteractiveElement(_cellList, "waypointsList")
            .EndClip();

        if (!_templatePack.Metadata.Custom) return;
        composer.AddSmallButton(LangEntry("AddNew"), OnAddNewWaypointTypeButtonPressed, buttonRowBoundsRightFixed);
    }

    private void AddSearchBox(GuiComposer composer, ref ElementBounds bounds)
    {
        const int switchSize = 30;
        const int gapBetweenRows = 20;
        var font = CairoFont.WhiteSmallText();
        var lblSearchText = $"{Lang.Get("Search")}:";
        var lblDisabledOnlyText = LangEntry("lblDisabledOnly");

        var lblSearchTextLength = font.GetTextExtents(lblSearchText).Width + 10;
        var lblDisabledOnlyTextLength = font.GetTextExtents(lblDisabledOnlyText).Width + 10;

        var left = ElementBounds.Fixed(0, 5, lblSearchTextLength, switchSize).FixedUnder(bounds, 3);
        var right = ElementBounds.Fixed(lblSearchTextLength + 10, 0, 200, switchSize).FixedUnder(bounds, 3);

        composer.AddStaticText(lblSearchText, font, EnumTextOrientation.Left, left);
        composer.AddAutoSizeHoverText(LangEntry("lblSearch.HoverText"), font, 160, left);
        composer.AddTextInput(right, OnFilterTextChanged);

        right = ElementBounds.FixedSize(EnumDialogArea.RightFixed, switchSize, switchSize).FixedUnder(bounds, 3);
        left = ElementBounds.FixedSize(EnumDialogArea.RightFixed, lblDisabledOnlyTextLength, switchSize).FixedUnder(bounds, 8).WithFixedOffset(-40, 0);

        composer.AddStaticText(lblDisabledOnlyText, font, EnumTextOrientation.Left, left);
        composer.AddSwitch(OnCurrentlyPlayingToggle, right);

        bounds = bounds.BelowCopy(fixedDeltaY: gapBetweenRows);
    }

    private void OnCurrentlyPlayingToggle(bool state)
    {
        _disabledTypesOnly = state;
        FilterCells();
        RefreshValues();
    }

    private void OnFilterTextChanged(string filterString)
    {
        _filterString = filterString;
        FilterCells();
        RefreshValues();
    }

    private void FilterCells()
    {
        bool Filter(IGuiElementCell cell)
        {
            var c = (PredefinedWaypointsGuiCell)cell;
            var model = c.Model;
            var state = string.IsNullOrWhiteSpace(_filterString) ||
                        model.Title.ToLowerInvariant().Contains(_filterString.ToLowerInvariant()) ||
                        model.Key.ToLowerInvariant().Contains(_filterString.ToLowerInvariant());

            if (_disabledTypesOnly && c.On) state = false;

            return state;
        }

        _cellList.CallMethod("FilterCells", (System.Func<IGuiElementCell, bool>)Filter);
    }

    private bool OnAddNewWaypointTypeButtonPressed()
    {
        var template = new PredefinedWaypointTemplate().With(p => p.TemplatePack = _templatePack);
        var dialogue = IOC.Services.CreateInstance<AddEditWaypointTypeDialogue>(template, CrudAction.Add).With(p =>
        {
            p.Title = LangEntry("AddNew");
            p.OnOkAction = AddNewWaypointType;
        });
        dialogue.TryOpen();
        return true;
    }

    private void AddNewWaypointType(PredefinedWaypointTemplate model)
    {
        if (_templates.ContainsKey(model.Key))
        {
            MessageBox.Show(LangEntry("Error"), LangEntry("AddNew.Validation", model.Key));
            return;
        }
        _templates.Add(model.Key, model);

        _templatePack.Templates.Add(model);
        var file = _fileSystemService.GetJsonFile($"{_templatePack.Metadata.Name}.json");
        file.SaveFrom(_templatePack);

        _cellList.ReloadCells(_waypointCells = GetCellEntries());
        FilterCells();
        RefreshValues();
    }

    #endregion

    #region Cell List Management

    private IGuiElementCell CellCreator(PredefinedWaypointsCellEntry cell, ElementBounds bounds)
    {
        return new PredefinedWaypointsGuiCell(ApiEx.Client, cell, bounds)
        {
            On = cell.Model.Enabled = cell.Enabled,
            OnMouseDownOnCellLeft = OnCellClickLeftSide,
            OnMouseDownOnCellRight = OnCellClickRightSide
        };
    }

    private List<PredefinedWaypointsCellEntry> GetCellEntries()
    {
        if (!_templates.Any()) return [];
        var list = _templates.Select(kvp =>
        {
            var dto = kvp.Value;
            return new PredefinedWaypointsCellEntry
            {
                Title = dto.Title,
                DetailText = string.Empty,
                Enabled = !_settings.DisabledTemplatesPerPack.ListContains(_templatePack.Metadata.Name, dto.Key),
                RightTopText = dto.Key,
                RightTopOffY = 3f,
                DetailTextFont = CairoFont.WhiteDetailText().WithFontSize((float)GuiStyle.SmallFontSize),
                Model = dto.With(p => p.TemplatePack = _templatePack)
            };
        }).ToList();
        return list;
    }

    #endregion

    #region Control Event Handlers

    private void OnScroll(float dy)
    {
        var bounds = SingleComposer.GetElement("waypointsList").Bounds;
        bounds.fixedY = 0f - dy;
        bounds.CalcWorldBounds();
    }

    private void OnCellClickLeftSide(int val)
    {
        if (!_templatePack.Metadata.Custom) return;
        var cell = _cellList.elementCells.Cast<PredefinedWaypointsGuiCell>().ToList()[val];
        var dialogue = IOC.Services.CreateInstance<AddEditWaypointTypeDialogue>(cell.Model, CrudAction.Edit).With(p =>
        {
            p.Title = LangEntry("Edit");
            p.OnOkAction = EditWaypointType;
            p.OnDeleteAction = DeleteWaypointType;
            p.OnScopeChange = TryClose;
        });
        dialogue.TryOpen();
    }

    private void DeleteWaypointType(PredefinedWaypointTemplate model)
    {
        var title = LangEx.FeatureString("PredefinedWaypoints.Dialogue.WaypointType", "Delete.Title");
        var message = LangEx.FeatureString("PredefinedWaypoints.Dialogue.WaypointType", "Delete.Message");
        MessageBox.Show(title, message, ButtonLayout.OkCancel,
            () =>
            {
                if (!_templates.ContainsKey(model.Key))
                {
                    MessageBox.Show(LangEntry("Error"), LangEntry("Edit.Validation", model.Key));
                    return;
                }
                _templates.Remove(model.Key);
                _templatePack.Templates.RemoveAll(p => p.Key == model.Key);
                var file = _fileSystemService.GetJsonFile($"{_templatePack.Metadata.Name}.json");
                file.SaveFrom(_templatePack);
                _cellList.ReloadCells(_waypointCells = GetCellEntries());
                FilterCells();
                RefreshValues();
            });
    }

    private void EditWaypointType(PredefinedWaypointTemplate model)
    {
        if (!_templates.ContainsKey(model.Key))
        {
            MessageBox.Show(LangEntry("Error"), LangEntry("Edit.Validation", model.Key));
            return;
        }
        _templates[model.Key] = model;
        var index = _templatePack.Templates.FindIndex(p => p.Key == model.Key);
        _templatePack.Templates[index] = model;
        var file = _fileSystemService.GetJsonFile($"{_templatePack.Metadata.Name}.json");
        file.SaveFrom(_templatePack);
        _cellList.ReloadCells(_waypointCells = GetCellEntries());
        FilterCells();  
        RefreshValues();
    }

    private void OnCellClickRightSide(int val)
    {
        var cell = _cellList.elementCells.Cast<PredefinedWaypointsGuiCell>().ToList()[val];
        cell.On = !cell.On;
        cell.Enabled = cell.On;
        cell.Model.Enabled = cell.On;

        if (cell.On)
        {
            _settings.DisabledTemplatesPerPack.RemoveFromList(_templatePack.Metadata.Name, cell.Model.Key);
        }
        else
        {
            _settings.DisabledTemplatesPerPack.AddToList(_templatePack.Metadata.Name, cell.Model.Key);
        }

        ModSettings.World.Save(_settings);
        
        RefreshValues();
    }

    #endregion

    public override bool TryClose()
    {
        IOC.Services.Resolve<WaypointTemplateService>().LoadWaypointTemplates();
        OnClose();
        return base.TryClose();
    }

    public Action OnClose { get; set; }
}