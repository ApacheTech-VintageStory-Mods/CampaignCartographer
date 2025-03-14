using ApacheTech.Common.Extensions.Harmony;
using ApacheTech.VintageMods.CampaignCartographer.Features.WaypointBeacons;
using ApacheTech.VintageMods.CampaignCartographer.Features.WaypointGroups;
using ApacheTech.VintageMods.CampaignCartographer.Features.WaypointGroups.Models;
using Gantry.Core.GameContent.GUI.Abstractions;
using Gantry.Core.GameContent.GUI.Models;
using Gantry.Services.FileSystem.Configuration;
using Gantry.Services.Network.Packets;
using Gantry.Services.Network;
using Vintagestory.API.MathTools;
using Vintagestory.API.Util;
using Vintagestory.API.Server;
using Groups = ApacheTech.VintageMods.CampaignCartographer.Features.WaypointGroups.Systems.WaypointGroups;

namespace ApacheTech.VintageMods.CampaignCartographer.Features.WaypointManager.Dialogues;

public class AddEditWaypointDialogue : GenericDialogue
{
    private readonly string[] _icons;
    private readonly int[] _colours;
    private readonly Waypoint _waypoint;
    private readonly int _index;
    private readonly BlockPos _position;
    private readonly AddEditDialogueMode _mode;
    private readonly IClientNetworkChannel _clientChannel;
    private readonly IPlayer[] _onlinePlayers;
    private readonly Dictionary<string, string> _waypointGroups;
    private readonly WaypointGroupsSettings _waypointGroupsSettings;
    private readonly WaypointBeaconsSettings _waypointBeaconSettings;

    private bool _beacon;
    private bool _autoSuggest = true;
    private bool _ignoreNextAutoSuggestDisable;
    private string _selectedGroupId;
    private WaypointGroup _group;

    public AddEditWaypointDialogue(ICoreClientAPI capi, Waypoint waypoint, int index)
        : this(capi, AddEditDialogueMode.Edit, waypoint, index: index)
    {
    }

    public AddEditWaypointDialogue(ICoreClientAPI capi, BlockPos position)
        : this(capi, AddEditDialogueMode.Add, null, position: position)
    {
        _waypoint = new Waypoint
        {
            Guid = Guid.NewGuid().ToString(),
            OwningPlayerUid = capi.World.Player.PlayerUID,
            Icon = "circle",
            Color = ColorUtil.BlackArgb,
            Title = "",
            Text = "",
            Position = position.ToVec3d()
        };
    }

    private AddEditWaypointDialogue(        
        ICoreClientAPI capi,
        AddEditDialogueMode mode,
        Waypoint waypoint,
        int index = 0,
        BlockPos position = null)
        : base(capi)
    {
        var waypointMapLayer = IOC.Services.GetRequiredService<WaypointMapLayer>();
        _icons = [.. waypointMapLayer.WaypointIcons.Keys];
        _colours = [.. waypointMapLayer.WaypointColors];
        _waypoint = waypoint.DeepClone();
        _index = index;
        _position = position;
        _mode = mode;
        _waypointGroupsSettings = IOC.Services.Resolve<WaypointGroupsSettings>();
        _waypointBeaconSettings = IOC.Services.Resolve<WaypointBeaconsSettings>();
        _onlinePlayers = [.. capi.World.AllOnlinePlayers.Except([capi.World.Player])];        
        _waypointGroups = Groups.GetWaypointGroupListItems();

        _clientChannel = capi.Network.GetChannel(nameof(WaypointManager));

        Title = T($"{_mode}.Title");
        Alignment = EnumDialogArea.CenterMiddle;
        Modal = true;
        ModalTransparency = .4f;
    }

    protected override void RefreshValues()
    {
        _beacon = _waypointBeaconSettings.ActiveBeacons.Contains(_waypoint.Guid);
        var txtTitle = SingleComposer.GetTextInput("txtTitle");
        txtTitle.SetValue(_waypoint.Title);
        SingleComposer.GetTextInput("txtDescription").SetValue(_waypoint.Text);
        SingleComposer.GetSwitch("btnPinned").SetValue(_waypoint.Pinned);
        SingleComposer.ColorListPickerSetValue("optColour", Math.Max(_colours.IndexOf(_waypoint.Color), 0));
        SingleComposer.IconListPickerSetValue("optIcon", Math.Max(_icons.IndexOf(_waypoint.Icon), 0));

        txtTitle.SetField("hasFocus", false);
        SingleComposer.GetSwitch("btnBeacon").SetValue(_beacon);
        if (_waypointGroups.Count > 1)
        {
            _group = Groups.GetWaypointGroup(_waypoint);
            _selectedGroupId = _group?.Id.ToString();
            var index = _waypointGroups.Keys.IndexOf(p => p == _selectedGroupId);
            SingleComposer.GetDropDown("btnWaypointGroup").SetSelectedIndex(Math.Max(0, index));
        }
    }

    protected override void ComposeBody(GuiComposer composer)
    {
        var labelFont = CairoFont.WhiteSmallText();
        var txtTitleFont = CairoFont.WhiteDetailText();
        var topBounds = ElementBounds.FixedSize(600, 30);

        //
        // Title
        //

        var left = ElementBounds.FixedSize(100, 30).FixedUnder(topBounds, 10);
        var right = ElementBounds.FixedSize(470, 30).FixedUnder(topBounds, 10).FixedRightOf(left, 10);

        composer
            .AddStaticText(T("lblTitle"), labelFont, EnumTextOrientation.Right, left, "lblTitle")
            .AddAutoSizeHoverText(T("lblTitle.HoverText"), txtTitleFont, 260, left)
            .AddTextInput(right, OnTitleChanged, txtTitleFont, "txtTitle");

        //
        // Text
        //

        left = ElementBounds.FixedSize(100, 30).FixedUnder(left, 10);
        right = ElementBounds.FixedSize(470, 30).FixedUnder(right, 10).FixedRightOf(left, 10);

        composer
            .AddStaticText(T("lblDescription"), labelFont, EnumTextOrientation.Right, left, "lblDescription")
            .AddAutoSizeHoverText(T("lblDescription.HoverText"), txtTitleFont, 260, left)
            .AddTextInput(right, OnDescriptionChanged, txtTitleFont, "txtDescription");

        //
        // Waypoint Group
        //

        if (_waypointGroups.Count > 1) 
        {
            left = ElementBounds.FixedSize(100, 30).FixedUnder(left, 10);
            right = ElementBounds.FixedSize(470, 30).FixedUnder(right, 10).FixedRightOf(left, 10);

            var keys = _waypointGroups.Keys.ToArray();
            var values = _waypointGroups.Values.ToArray();

            composer
                .AddStaticText(T("lblWaypointGroup"), labelFont, EnumTextOrientation.Right, left, "lblWaypointGroup")
                .AddAutoSizeHoverText(T("lblWaypointGroup.HoverText"), txtTitleFont, 260, left)
                .AddDropDown(keys, values, 0, OnWaypointGroupChanged, right, CairoFont.WhiteSmallText(), "btnWaypointGroup");
        }

        //
        // Pinned
        //

        left = ElementBounds.FixedSize(100, 30).FixedUnder(left, 10);
        right = ElementBounds.FixedSize(270, 30).FixedUnder(right, 10).FixedRightOf(left, 10);

        composer
            .AddStaticText(T("lblPinned"), labelFont, EnumTextOrientation.Right, left, "lblPinned")
            .AddAutoSizeHoverText(T("lblPinned.HoverText"), txtTitleFont, 260, left)
            .AddSwitch(OnPinnedChanged, right, "btnPinned");

        //
        // Beacon
        //

        left = ElementBounds.FixedSize(100, 30).FixedUnder(left, 10);
        right = ElementBounds.FixedSize(270, 30).FixedUnder(right, 10).FixedRightOf(left, 10);

        composer
            .AddStaticText(T("lblBeacon"), labelFont, EnumTextOrientation.Right, left, "lblBeacon")
            .AddAutoSizeHoverText(T("lblBeacon.HoverText"), txtTitleFont, 260, left)
            .AddSwitch(OnBeaconChanged, right, "btnBeacon");

        //
        // Colour
        //

        const double colourIconSize = 22d;
        left = ElementBounds.FixedSize(100, 30).FixedUnder(left, 10);
        right = ElementBounds.FixedSize(270, 30).FixedUnder(right, 10).FixedRightOf(left, 10);

        composer
            .AddStaticText(T("lblColour"), labelFont, EnumTextOrientation.Right, left, "lblColour")
            .AddAutoSizeHoverText(T("lblColour.HoverText"), txtTitleFont, 260, left)
            .AddColorListPicker(_colours, OnColourSelected, right.WithFixedSize(colourIconSize, colourIconSize), 470, "optColour");

        //
        // Icon
        //

        const double iconIconSize = 27d;
        left = ElementBounds.FixedSize(100, 30).FixedUnder(right, 10);
        right = ElementBounds.FixedSize(270, 30).FixedUnder(right, 10).FixedRightOf(left, 10);

        composer
            .AddStaticText(T("lblIcon"), labelFont, EnumTextOrientation.Right, left, "lblIcon")
            .AddAutoSizeHoverText(T("lblIcon.HoverText"), txtTitleFont, 260, left)
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

        //
        // Delete Button
        //

        if (_mode == AddEditDialogueMode.Add) return;
        buttonBounds = buttonBounds.FlatCopy().FixedLeftOf(buttonBounds, 10);
        composer.AddSmallButton(LangEx.ConfirmationString("delete"), OnDeleteButtonPressed, buttonBounds, EnumButtonStyle.Normal, "btnDelete");

        //
        // Share Button
        //

        if (_onlinePlayers.Length > 0)
        {
            buttonBounds = buttonBounds.FlatCopy().FixedLeftOf(buttonBounds, 10);
            composer.AddSmallButton(T("btnShare"), OnShareButtonPressed, buttonBounds, EnumButtonStyle.Normal, "btnShare");
        }

        //
        // Teleport Button
        //

        if (capi.World.Player.HasPrivilege(Privilege.tp))
        {
            buttonBounds = buttonBounds.FlatCopy().FixedLeftOf(buttonBounds, 10);
            composer.AddSmallButton(T("btnTeleport"), OnTeleportButtonPressed, buttonBounds, EnumButtonStyle.Normal, "btnTeleport");
        }
    }

    private void OnWaypointGroupChanged(string code, bool selected)
    {
        _selectedGroupId = code;
    }

    private bool OnTeleportButtonPressed()
    {
        _clientChannel.SendPacket<WorldMapTeleportPacket>(new() { Position = _waypoint.Position });
        return TryClose();
    }

    private void AutoSuggestTitle()
    {   
        if (!_autoSuggest) return;
        var txtTitle = SingleComposer.GetTextInput("txtTitle");
        _ignoreNextAutoSuggestDisable = true;
        var suggestionKey = $"wpSuggestion-{_waypoint.Icon}-{ColorUtil.Int2HexRgba(_waypoint.Color)}";
        var value = suggestionKey switch
        {
            _ when Lang.HasTranslation(suggestionKey) => Lang.Get(suggestionKey),
            _ when Lang.HasTranslation($"wpSuggestion-{_waypoint.Icon}") => Lang.Get($"wpSuggestion-{_waypoint.Icon}"),
            _ => ""
        };
        txtTitle.SetValue(value);
    }

    private void OnIconSelected(int index)
    {
        _waypoint.Icon = _icons[index];
        AutoSuggestTitle();
    }

    private void OnColourSelected(int index)
    {
        _waypoint.Color = _colours[index];
        AutoSuggestTitle();
    }

    private void OnBeaconChanged(bool state)
    {
        _beacon = state;
    }

    private void OnPinnedChanged(bool state)
    {
        _waypoint.Pinned = state;
    }

    private void OnDescriptionChanged(string text)
    {
        _waypoint.Text = text;
    }

    private void OnTitleChanged(string title)
    {
        _waypoint.Title = title;
        SingleComposer.GetButton("btnSave").Enabled = !string.IsNullOrWhiteSpace(title);
        _autoSuggest = !_ignoreNextAutoSuggestDisable && string.IsNullOrWhiteSpace(title);
        _ignoreNextAutoSuggestDisable = false;
    }

    private bool OnDeleteButtonPressed()
    {
        MessageBox.Show(T("DeleteConfirmationTitle"), T("DeleteConfirmationMessage"), ButtonLayout.OkCancel, DeleteWaypoint);
        return true;
        void DeleteWaypoint()
        {
            if (_waypointBeaconSettings.ActiveBeacons.RemoveAll(p => p == _waypoint.Guid) > 0)
            {
                ModSettings.World.Save(_waypointBeaconSettings);
            }
            capi.SendChatMessage($"/waypoint remove {_index}");
            RemoveWaypointGroup();
            TryClose();
        }
    }

    private bool OnShareButtonPressed()
    {
        var dialogue = new ShareWaypointDialogue(capi, _onlinePlayers, [_waypoint]);
        dialogue.ToggleGui();
        return true;
    }

    private bool OnOkButtonPressed()
    {
        G.Logger.VerboseDebug($"{_mode}ing waypoint: {_waypoint.Guid}");
        _clientChannel.SendPacket<WaypointActionPacket>(new() { Mode = _mode, Waypoint = _waypoint });
                
        if (_beacon
            ? _waypointBeaconSettings.ActiveBeacons.AddIfNotPresent(_waypoint.Guid)
            : _waypointBeaconSettings.ActiveBeacons.RemoveIfPresent(_waypoint.Guid))
            ModSettings.World.Save(_waypointBeaconSettings);

        UpdateWaypointGroup();
        return TryClose();
    }

    private void RemoveWaypointGroup()
    {
        // 0. Don't do anything if the group is null.
        if (_group is null) return;

        // 1. Remove the waypoint from the current group.
        if (!Guid.TryParse(_waypoint.Guid, out var waypointId)) return;
        _group.Waypoints.RemoveIfPresent(waypointId);

        // 2. Save the changes.
        ModSettings.World.Save(_waypointGroupsSettings);
    }

    private void UpdateWaypointGroup()
    {
        // 0. Don't do anything if the group hasn't changed.
        if (_selectedGroupId == _group?.Id.ToString().IfNullOrEmpty(string.Empty)) return;

        // 1. Remove the waypoint from the current group.
        if (!Guid.TryParse(_waypoint.Guid, out var waypointId)) return;
        _group?.Waypoints.RemoveIfPresent(waypointId);

        // 2. Add the waypoint to the new group, if required.
        if (Guid.TryParse(_selectedGroupId, out var groupId))
        {
            var newGroup = _waypointGroupsSettings.Groups.FirstOrDefault(p => p.Id == groupId);
            newGroup?.Waypoints.AddIfNotPresent(waypointId);
        }

        // 3. Save the changes.
        ModSettings.World.Save(_waypointGroupsSettings);
    }

    private bool OnCancelButtonPressed()
    {
        return TryClose();
    }

    #region Overrides

    public override bool CaptureAllInputs() => IsOpened();
    public override EnumDialogType DialogType => EnumDialogType.Dialog;
    public override bool DisableMouseGrab => true;
    public override double DrawOrder => 0.2;
    public override bool PrefersUngrabbedMouse => true;

    #endregion

    private static string T(string path, params IEnumerable<string> args)
        => LangEx.FeatureString("WaypointManager.Dialogue", path, args);
}
