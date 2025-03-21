using Gantry.Core.GameContent.GUI.Abstractions;
using Gantry.Services.Network;

namespace ApacheTech.VintageMods.CampaignCartographer.Features.TeleporterService.Dialogue;

/// <summary>
///     Represents a dialogue window for managing teleporter locations.
/// </summary>
public class TeleporterLocationDialogue : GenericDialogue
{
    private readonly List<TeleporterLocation> _locations;
    private readonly TpLocations _locationData;
    private readonly string[] _targets;

    /// <summary>
    ///     Initialises a new instance of the <see cref="TeleporterLocationDialogue" /> class.
    /// </summary>
    /// <param name="capi">The core client API.</param>
    /// <param name="locationData">The data for the current teleporter location.</param>
    public TeleporterLocationDialogue(ICoreClientAPI capi, TpLocations locationData) : base(capi)
    {
        Title = T("Title");
        Alignment = EnumDialogArea.CenterMiddle;
        Modal = true;
        ModalTransparency = .4f;

        _locationData = locationData;
        _locations = [.. locationData.Locations.Values];
        _targets = [.. _locations.Select(p => p.SourceName)];
    }

    /// <summary>
    ///     Refreshes the input values within the dialogue.
    /// </summary>
    protected override void RefreshValues()
    {
        var txtName = SingleComposer.GetTextInput("txtName");
        var txtNameValue = _locationData.ForLocation.SourceName;
        txtName.SetValue(txtNameValue);

        var index = _locations.FindIndex(p => p.SourcePos == _locationData.ForLocation.TargetPos);
        var selectedIndex = Math.Max(0, index);
        var cbxTargetLocation = SingleComposer.GetDropDown("cbxTargetLocation");
        cbxTargetLocation.SetSelectedIndex(selectedIndex);
    }

    /// <summary>
    ///     Composes the GUI body of the dialogue.
    /// </summary>
    /// <param name="composer">The GUI composer instance.</param>
    protected override void ComposeBody(GuiComposer composer)
    {
        var labelFont = CairoFont.WhiteSmallText();
        var txtTitleFont = CairoFont.WhiteDetailText();
        var topBounds = ElementBounds.FixedSize(400, 30);

        //
        // Name
        //

        var left = ElementBounds.FixedSize(100, 30).FixedUnder(topBounds, 10);
        var right = ElementBounds.FixedSize(270, 30).FixedUnder(topBounds, 10).FixedRightOf(left, 10);

        composer
            .AddStaticText(T("lblName"), labelFont, EnumTextOrientation.Right, left, "lblName")
            .AddAutoSizeHoverText(T("lblName.HoverText"), txtTitleFont, 260, left)
            .AddTextInput(right, OnNameChanged, txtTitleFont, "txtName");

        //
        // Target Location
        //

        left = ElementBounds.FixedSize(100, 30).FixedUnder(left, 10);
        right = ElementBounds.FixedSize(270, 30).FixedUnder(right, 10).FixedRightOf(left, 10);

        composer
            .AddStaticText(T("lblTargetLocation"), labelFont, EnumTextOrientation.Right, left, "lblTargetLocation")
            .AddHoverText(T("lblTargetLocation.HoverText"), txtTitleFont, 260, left)
            .AddDropDown(_targets, _targets, 0, OnSelectedTargetChanged, right, txtTitleFont, "cbxTargetLocation");

        //
        // OK Button
        //

        var buttonBounds = ElementBounds.FixedSize(100, 30).WithAlignment(EnumDialogArea.RightFixed).FixedUnder(right, 10);
        composer.AddSmallButton(LangEx.ConfirmationString("ok"), OnOkButtonPressed, buttonBounds, EnumButtonStyle.Normal, "btnSave");

        //
        // Cancel Button
        //

        buttonBounds = buttonBounds.FlatCopy().FixedLeftOf(buttonBounds, 10);
        composer.AddSmallButton(LangEx.ConfirmationString("cancel"), OnCancelButtonPressed, buttonBounds, EnumButtonStyle.Normal, "btnCancel");
    }

    private void OnNameChanged(string name)
    {
        _locationData.ForLocation.SourceName = name;
    }

    private void OnSelectedTargetChanged(string targetName, bool selected)
    {
        var destination = _locations.First(p => p.SourceName == targetName);
        _locationData.ForLocation.TargetName = destination.SourceName;
        _locationData.ForLocation.TargetPos = destination.SourcePos;
    }

    private bool OnOkButtonPressed()
    {
        capi.Network.GetChannel("tpManager")?.SendPacket(_locationData.ForLocation);
        IOC.Services
            .GetRequiredService<FastTravelOverlay.FastTravelOverlay>()
            .UpdateTeleporterTarget(_locationData.ForLocation);

        return TryClose();
    }

    private bool OnCancelButtonPressed()
    {
        return TryClose();
    }

    private static string T(string path, params object[] args)
        => LangEx.FeatureString("TeleporterService.Dialogue", path, args);
}