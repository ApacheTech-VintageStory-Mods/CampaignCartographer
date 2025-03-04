#nullable enable
using System.Text;
using ApacheTech.Common.Extensions.Harmony;
using ApacheTech.VintageMods.CampaignCartographer.Features.WaypointGroups.MapLayers.Commands;
using ApacheTech.VintageMods.CampaignCartographer.Features.WaypointGroups.Models;
using Gantry.Core.Extensions.Helpers;
using Gantry.Core.GameContent.GUI.Abstractions;
using Gantry.Core.Hosting.Annotation;
using Gantry.Services.FileSystem.Configuration;

namespace ApacheTech.VintageMods.CampaignCartographer.Features.WaypointGroups.Dialogue;

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public class AddEditWaypointGroupDialogue : GenericDialogue
{
    private readonly WaypointGroupsSettings _settings;
    private readonly WaypointGroup _group;
    private readonly AddEditDialogueMode _mode;

    public required Action OnChanged { get; init; }

    /// <summary>
    /// 	Initialises a new instance of the <see cref="AddEditWaypointGroupDialogue"/> class.
    /// </summary>
    /// <param name="group">The waypoint group.</param>
    /// <param name="mode">Whether we are adding or editing a group</param>
    [SidedConstructor(EnumAppSide.Client)]
    public AddEditWaypointGroupDialogue(WaypointGroup? group = null) : base(ApiEx.Client)
    {
        _mode = group is null ? AddEditDialogueMode.Add : AddEditDialogueMode.Edit;
        _group = group?.DeepClone() ?? new();
        _settings = IOC.Services.GetRequiredService<WaypointGroupsSettings>();

        Title = T(_mode == AddEditDialogueMode.Add ? "AddNew" : "Edit");
        Alignment = EnumDialogArea.CenterMiddle;
        Modal = true;
        ModalTransparency = .4f;
    }

    #region Form Composition

    protected override void RefreshValues()
    {
        SingleComposer.GetTextInput("txtTitle").SetValue(_group.Title);
    }

    protected override void ComposeBody(GuiComposer composer)
    {
        var labelFont = CairoFont.WhiteSmallText();
        var txtTitleFont = CairoFont.WhiteDetailText();
        var topBounds = ElementBounds.FixedSize(400, 30);

        //
        // Title
        //

        var left = ElementBounds.FixedSize(100, 30).FixedUnder(topBounds, 10);
        var right = ElementBounds.FixedSize(270, 30).FixedUnder(topBounds, 10).FixedRightOf(left, 10);

        composer
            .AddStaticText(T("lblTitle"), labelFont, EnumTextOrientation.Right, left, "lblTitle")
            .AddAutoSizeHoverText(T("lblTitle.HoverText"), txtTitleFont, 260, left)
            .AddTextInput(right, OnTitleChanged, txtTitleFont, "txtTitle");

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

        //
        // Delete Button
        //

        if (_mode == AddEditDialogueMode.Edit)
        {
            buttonBounds = buttonBounds.FlatCopy().FixedLeftOf(buttonBounds, 10);
            composer.AddSmallButton(LangEx.ConfirmationString("delete"), OnDeleteButtonPressed, buttonBounds, EnumButtonStyle.Normal, "btnDelete");
        }
    }

    #endregion

    #region Control Event Handlers

    private void OnTitleChanged(string title)
    {
        _group.Title = title;
    }

    private bool OnCancelButtonPressed()
    {
        return TryClose();
    }

    private bool OnOkButtonPressed()
    {
        var validationErrors = false;
        var message = new StringBuilder();

        if (string.IsNullOrWhiteSpace(_group.Title))
        {
            message.AppendLine(T("lblTitle.ValidationError"));
            validationErrors = true;
        }

        if (validationErrors)
        {
            var title = LangEx.Get("ModTitle");
            MessageBox.Show(title, message.ToString());
            return false;
        }

        _mode.Switch(
            (AddEditDialogueMode.Add, AddWaypointGroup),
            (AddEditDialogueMode.Edit, UpdateWaypointGroup)
        );
        return TryClose();
    }

    private void AddWaypointGroup()
    {
        _settings.Groups.Add(_group);
        IOC.Brighter.Send(new AddWaypointGroupLayerCommand { Group = _group });
        ModSettings.World.Save(_settings);
        OnChanged();
    }

    private void UpdateWaypointGroup()
    {
        _settings.Groups.First(p => p.Id == _group.Id).Title = _group.Title;
        IOC.Brighter.Send(new UpdateWaypointGroupLayerCommand { Group = _group });
        ModSettings.World.Save(_settings);
        OnChanged();
    }

    private bool OnDeleteButtonPressed()
    {
        _settings.Groups.RemoveAll(p => p.Id == _group.Id);
        ModSettings.World.Save(_settings);
        IOC.Brighter.Send(new RemoveWaypointGroupLayerCommand { GroupId = _group.Id.ToString() });
        OnChanged();
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
        => LangEx.FeatureString($"WaypointGroups.Dialogue", path, args);
}