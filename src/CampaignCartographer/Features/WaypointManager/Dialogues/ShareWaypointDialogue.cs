using Gantry.Core.GameContent.GUI.Abstractions;
using ApacheTech.VintageMods.CampaignCartographer.Features.WaypointSharing;

namespace ApacheTech.VintageMods.CampaignCartographer.Features.WaypointManager.Dialogues;

internal class ShareWaypointDialogue : GenericDialogue
{
    private readonly WaypointSharingService _waypointSharingService;
    private readonly IEnumerable<Waypoint> _waypoints;
    private readonly IPlayer[] _onlinePlayers;
    private IPlayer _selectedPlayer;

    /// <inheritdoc />
    public override double DrawOrder => 1;

    /// <inheritdoc />
    public override bool CaptureAllInputs() => true;

    /// <inheritdoc />
    public override double InputOrder => 0;

    /// <inheritdoc />
    public override EnumDialogType DialogType => EnumDialogType.Dialog;

    /// <inheritdoc />
    public override bool CaptureRawMouse() => true;

    public ShareWaypointDialogue(ICoreClientAPI capi, IPlayer[] onlinePlayers, IEnumerable<Waypoint> waypoints) : base(capi)
    {
        _waypoints = waypoints;
        _onlinePlayers = onlinePlayers;
        _selectedPlayer = _onlinePlayers[0];
        _waypointSharingService = IOC.Services.GetRequiredService<WaypointSharingService>();

        Title = T("Title");
        Alignment = EnumDialogArea.CenterMiddle;
        Modal = true;
        ModalTransparency = .4f;
    }

    protected override void RefreshValues()
    {
        if (!IsOpened()) return;
        SingleComposer.GetDropDown("optPlayerSelection").SetSelectedIndex(0);
    }

    protected override void ComposeBody(GuiComposer composer)
    {
        var labelFont = CairoFont.WhiteSmallText();
        var txtTitleFont = CairoFont.WhiteDetailText();
        var topBounds = ElementBounds.FixedSize(600, 30);

        //
        // Player Selection
        //

        var left = ElementBounds.FixedSize(100, GuiStyle.TitleBarHeight + 1.0).FixedUnder(topBounds, 10);
        var right = ElementBounds.FixedSize(270, GuiStyle.TitleBarHeight + 1.0).FixedUnder(topBounds, 10).FixedRightOf(left, 10);

        var names = _onlinePlayers.Select(p => p.PlayerName).ToArray();
        composer.AddStaticText(T("optPlayerSelection"), labelFont, left);
        composer.AddHoverText(T("optPlayerSelection.HoverText"), labelFont, 260, left);
        composer.AddDropDown(names, names, 0, OnPlayerSelectionChanged, right, txtTitleFont, "optPlayerSelection");

        //
        // Buttons
        //

        left = ElementBounds.FixedSize(100, 30).FixedUnder(right, 10);
        right = ElementBounds.FixedSize(270, 30).FixedUnder(right, 10).FixedRightOf(left, 10);
        composer.AddSmallButton(T("btnShare", _selectedPlayer.PlayerName), OnShare, right, EnumButtonStyle.Normal, "btnShare");

        if (names.Length > 0)
        {
            left = ElementBounds.FixedSize(100, 30).FixedUnder(right, 10);
            right = ElementBounds.FixedSize(270, 30).FixedUnder(right, 10).FixedRightOf(left, 10);
            composer.AddSmallButton(T("btnBroadcast"), OnBroadcast, right, EnumButtonStyle.Normal, "btnBroadcast");
        }

        left = ElementBounds.FixedSize(100, 30).FixedUnder(right, 10);
        right = ElementBounds.FixedSize(270, 30).FixedUnder(right, 10).FixedRightOf(left, 10);
        composer.AddSmallButton(LangEx.ConfirmationString("cancel"), TryClose, right, EnumButtonStyle.Normal, "btnCancel");
    }

    private bool OnBroadcast()
    {
        _waypointSharingService.BroadcastWaypoints(_waypoints);
        return TryClose();
    }

    private bool OnShare()
    {
        _waypointSharingService.ShareWaypoints(_waypoints, _selectedPlayer.PlayerUID);
        return TryClose();
    }

    private void OnPlayerSelectionChanged(string playerName, bool selected)
    {
        _selectedPlayer = _onlinePlayers.First(p => p.PlayerName == playerName);
    }

    private static string T(string path, params object[] args)
        => LangEx.FeatureString("WaypointManager.Dialogue.Share", path, args);
}