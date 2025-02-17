using System.Text;
using ApacheTech.VintageMods.CampaignCartographer.Features.ManualWaypoints.Model;
using ApacheTech.VintageMods.CampaignCartographer.Features.WaypointManager.WaypointTemplates;
using Gantry.Core.GameContent.GUI.Abstractions;
using Gantry.Core.GameContent.GUI.Models;
using Gantry.Core.Hosting.Annotation;
using Gantry.Services.FileSystem.Abstractions.Contracts;
using Gantry.Services.FileSystem.Enums;
using Vintagestory.API.MathTools;
using Vintagestory.API.Util;

namespace ApacheTech.VintageMods.CampaignCartographer.Features.ManualWaypoints.Dialogue;

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public class AddEditWaypointTypeDialogue : GenericDialogue
{
    private readonly PredefinedWaypointTemplate _template;
    private readonly IFileSystemService _fileSystemService;
    private readonly WaypointTypeMode _mode;

    private readonly string[] _icons;
    private readonly int[] _colours;

    /// <summary>
    /// 	Initialises a new instance of the <see cref="AddEditWaypointTypeDialogue"/> class.
    /// </summary>
    /// <param name="capi">The capi.</param>
    /// <param name="waypoint">The waypoint.</param>
    /// <param name="mode"></param>
    [Obsolete("Use Factory Method: WaypointInfoDialogue.ShowDialogue()")]
    [SidedConstructor(EnumAppSide.Client)]
    public AddEditWaypointTypeDialogue(ICoreClientAPI capi, IFileSystemService fileSystemService, PredefinedWaypointTemplate waypoint, WaypointTypeMode mode) : base(capi)
    {
        var waypointMapLayer = IOC.Services.GetRequiredService<WaypointMapLayer>();
        _template = waypoint.Clone().To<PredefinedWaypointTemplate>();
        _fileSystemService = fileSystemService;
        _mode = mode;
        _icons = [.. waypointMapLayer.WaypointIcons.Keys];
        _colours = [.. waypointMapLayer.WaypointColors];

        var titlePrefix = _mode == WaypointTypeMode.Add ? "AddNew" : "Edit";
        Title = T(titlePrefix);
        Alignment = EnumDialogArea.CenterMiddle;
        Modal = true;
        ModalTransparency = .4f;
    }

    public Action<PredefinedWaypointTemplate> OnOkAction { get; set; }
    public Action<PredefinedWaypointTemplate> OnDeleteAction { get; set; }
    public ActionConsumable OnScopeChange { get; set; }

    #region Form Composition

    protected override void RefreshValues()
    {
        if (_mode == WaypointTypeMode.Add)
        {
            SingleComposer.GetTextInput("txtSyntax").SetValue(_template.Key);
        }
        var colour = _colours.IndexOf(_template.Colour.ColourValue());
        var icon = _icons.IndexOf(_template.DisplayedIcon);
        SingleComposer.GetTextInput("txtTitle").SetValue(_template.Title);
        SingleComposer.ColorListPickerSetValue("optColour", Math.Max(colour, 0));
        SingleComposer.IconListPickerSetValue("optIcon", Math.Max(icon, 0));
        SingleComposer.GetSlider("txtHorizontalRadius").SetValues(_template.HorizontalCoverageRadius, 0, 50, 1);
        SingleComposer.GetSlider("txtVerticalRadius").SetValues(_template.VerticalCoverageRadius, 0, 50, 1);
        SingleComposer.GetSwitch("btnPinned").SetValue(_template.Pinned);
    }

    protected override void ComposeBody(GuiComposer composer)
    {
        var labelFont = CairoFont.WhiteSmallText();
        var txtTitleFont = CairoFont.WhiteDetailText();
        var topBounds = ElementBounds.FixedSize(600, 30);

        //
        // Syntax
        //

        var left = ElementBounds.FixedSize(100, 30).FixedUnder(topBounds, 10);
        var right = ElementBounds.FixedSize(270, 30).FixedUnder(topBounds, 10).FixedRightOf(left, 10);

        composer
            .AddStaticText(T("Syntax"), labelFont, EnumTextOrientation.Right, left.WithFixedOffset(0, 5), "lblSyntax")
            .AddAutoSizeHoverText(T("Syntax.HoverText"), txtTitleFont, 260, left)
            .AddIf(_mode == WaypointTypeMode.Add)
            .AddTextInput(right, OnSyntaxChanged, txtTitleFont, "txtSyntax")
            .EndIf()
            .AddIf(_mode == WaypointTypeMode.Edit)
            .AddStaticText(_template.Key, txtTitleFont, EnumTextOrientation.Left, right.WithFixedOffset(0, 5))
            .EndIf();

        //
        // Title
        //

        left = ElementBounds.FixedSize(100, 30).FixedUnder(left, 10);
        right = ElementBounds.FixedSize(470, 30).FixedUnder(right, 10).FixedRightOf(left, 10);

        composer
            .AddStaticText(T("WaypointTitle"), labelFont, EnumTextOrientation.Right, left, "lblTitle")
            .AddAutoSizeHoverText(T("WaypointTitle.HoverText"), txtTitleFont, 260, left)
            .AddTextInput(right, OnTitleChanged, txtTitleFont, "txtTitle");

        //
        // Horizontal Radius
        //

        left = ElementBounds.FixedSize(100, 30).FixedUnder(left, 10);
        right = ElementBounds.FixedSize(470, 30).FixedUnder(right, 10).FixedRightOf(left, 10);

        composer
            .AddStaticText(T("HCoverage"), labelFont, EnumTextOrientation.Right, left, "lblHorizontalRadius")
            .AddHoverText(T("HCoverage.HoverText"), txtTitleFont, 260, left)
            .AddLazySlider(OnHorizontalRadiusChanged, right.FlatCopy().WithFixedHeight(20), "txtHorizontalRadius");

        //
        // Vertical Radius
        //

        left = ElementBounds.FixedSize(100, 30).FixedUnder(left, 10);
        right = ElementBounds.FixedSize(470, 30).FixedUnder(right, 10).FixedRightOf(left, 10);

        composer
            .AddStaticText(T("VCoverage"), labelFont, EnumTextOrientation.Right, left, "lblVerticalRadius")
            .AddHoverText(T("VCoverage.HoverText"), txtTitleFont, 260, left)
            .AddLazySlider(OnVerticalRadiusChanged, right.FlatCopy().WithFixedHeight(20), "txtVerticalRadius");

        //
        // Pinned
        //

        left = ElementBounds.FixedSize(100, 30).FixedUnder(left, 10);
        right = ElementBounds.FixedSize(270, 30).FixedUnder(right, 10).FixedRightOf(left, 10);

        composer
            .AddStaticText(T("Pinned"), labelFont, EnumTextOrientation.Right, left, "lblPinned")
            .AddAutoSizeHoverText(T("Pinned.HoverText"), txtTitleFont, 260, left)
            .AddSwitch(OnPinnedChanged, right, "btnPinned");

        //
        // Colour
        //

        const double colourIconSize = 22d;
        left = ElementBounds.FixedSize(100, 30).FixedUnder(left, 10);
        right = ElementBounds.FixedSize(270, 30).FixedUnder(right, 10).FixedRightOf(left, 10);

        composer
            .AddStaticText(T("Colour"), labelFont, EnumTextOrientation.Right, left, "lblColour")
            .AddAutoSizeHoverText(T("Colour.HoverText"), txtTitleFont, 260, left)
            .AddColorListPicker(_colours, OnColourSelected, right.WithFixedSize(colourIconSize, colourIconSize), 470, "optColour");

        //
        // Icon
        //

        const double iconIconSize = 27d;
        left = ElementBounds.FixedSize(100, 30).FixedUnder(right, 10);
        right = ElementBounds.FixedSize(270, 30).FixedUnder(right, 10).FixedRightOf(left, 10);

        composer
            .AddStaticText(T("Icon"), labelFont, EnumTextOrientation.Right, left, "lblIcon")
            .AddAutoSizeHoverText(T("Icon.HoverText"), txtTitleFont, 260, left)
            .AddIconListPicker(_icons, OnIconSelected, right.WithFixedSize(iconIconSize, iconIconSize), 470, "optIcon");

        //
        // OK Button
        //

        left = ElementBounds.FixedSize(100, 30).FixedUnder(right);
        var buttonBounds = ElementBounds.FixedSize(100, 30).WithAlignment(EnumDialogArea.RightFixed).FixedUnder(left, 10);
        composer.AddSmallButton(LangEx.ConfirmationString("ok"), OnOkButtonPressed, buttonBounds, EnumButtonStyle.Normal, "btnSave");

        //
        // Cancel Button
        //

        buttonBounds = buttonBounds.FlatCopy().FixedLeftOf(buttonBounds, 10);
        composer.AddSmallButton(LangEx.ConfirmationString("cancel"), OnCancelButtonPressed, buttonBounds, EnumButtonStyle.Normal, "btnCancel");
        if (_mode == WaypointTypeMode.Add) return;

        //
        // Delete Button
        //

        buttonBounds = buttonBounds.FlatCopy().FixedLeftOf(buttonBounds, 10);
        composer.AddSmallButton(LangEx.ConfirmationString("delete"), OnDeleteButtonPressed, buttonBounds, EnumButtonStyle.Normal, "btnDelete");

        //
        // Change Scope Button
        //

        buttonBounds = buttonBounds.FlatCopy().FixedLeftOf(buttonBounds, 10).WithFixedWidth(150);
        composer.AddSmallButton(T("ChangeScope"), OnChangeScopeButtonPressed, buttonBounds, EnumButtonStyle.Normal, "btnChangeScope");
    }

    #endregion

    #region Control Event Handlers

    private bool OnChangeScopeButtonPressed()
    {
        var currentScope = _template.TemplatePack.Metadata.Scope;
        var targetScope = currentScope == FileScope.World ? FileScope.Global : FileScope.World;
        var message = T($"ChangeScope.To{targetScope}");

        MessageBox.Show(T("ChangeScope"), message, ButtonLayout.OkCancel, () =>
        {
            var currentPack = Systems.PredefinedWaypoints.CustomPacks[currentScope];
            var targetPack = Systems.PredefinedWaypoints.CustomPacks[targetScope];

            currentPack.Templates.Remove(_template);
            _template.TemplatePack = targetPack;
            targetPack.Templates.Add(_template);

            Save(currentPack);
            Save(targetPack);

            TryClose();
            OnScopeChange();
        });
        return true;
        void Save(TemplatePack pack)
        {
            var file = _fileSystemService.GetJsonFile($"{pack.Metadata.Name}.json");
            file.SaveFrom(pack);
        }
    }

    private void OnSyntaxChanged(string syntax)
    {
        _template.Key = syntax.ToLowerInvariant();
    }

    private void OnTitleChanged(string title)
    {
        _template.Title = title;
    }

    private void OnIconSelected(int index)
    {
        _template.DisplayedIcon = _icons[index];
        _template.ServerIcon = _icons[index];
    }

    private void OnColourSelected(int index)
    {
        _template.Colour = ColorUtil.Int2Hex(_colours[index]);
    }

    private bool OnHorizontalRadiusChanged(int radius)
    {
        _template.HorizontalCoverageRadius = radius;
        return true;
    }

    private bool OnVerticalRadiusChanged(int radius)
    {
        _template.VerticalCoverageRadius = radius;
        return true;
    }

    private void OnPinnedChanged(bool state)
    {
        _template.Pinned = state;
    }

    private bool OnCancelButtonPressed()
    {
        return TryClose();
    }

    private bool OnOkButtonPressed()
    {
        // ROADMAP: O/C issues with validation. FluentValidation???
        var validationErrors = false;
        var message = new StringBuilder();

        if (string.IsNullOrWhiteSpace(_template.Key) || _template.Key.Contains(' '))
        {
            message.AppendLine(T("Syntax.Validation"));
            message.AppendLine();
            validationErrors = true;
        }

        if (_template.HorizontalCoverageRadius < 0 || _template.VerticalCoverageRadius < 0)
        {
            message.AppendLine(T("Coverage.Validation"));
            message.AppendLine();
            validationErrors = true;
        }

        if (validationErrors)
        {
            var title = LangEx.Get("ModTitle");
            MessageBox.Show(title, message.ToString());
            return false;
        }

        OnOkAction?.Invoke(_template);
        return TryClose();
    }

    private bool OnDeleteButtonPressed()
    {
        OnDeleteAction?.Invoke(_template);
        return TryClose();
    }

    #endregion

    /// <summary>
    ///     Gets an entry from the language files, for the feature this instance is representing.
    /// </summary>
    /// <param name="path">The entry to return.</param>
    /// <param name="args">The entry to return.</param>
    /// <returns>A localised <see cref="string"/>, for the specified language file code.</returns>
    protected static string T(string path, params object[] args)
        => LangEx.FeatureString($"PredefinedWaypoints.Dialogue.WaypointType", path, args);
}