using System.Collections.Concurrent;
using ApacheTech.VintageMods.CampaignCartographer.Features.WaypointBeacons.Dialogue;
using Gantry.Core.Extensions.Api;
using Gantry.Core.Extensions.DotNet;
using Gantry.Services.FileSystem.Configuration;
using Vintagestory.API.Util;

// ReSharper disable InconsistentNaming

namespace ApacheTech.VintageMods.CampaignCartographer.Features.WaypointBeacons;

/// <summary>
///     Manages the waypoint beacons system on the client side, including updating, repopulating, 
///     and rendering the waypoint elements.
/// </summary>
/// <remarks>
///     The system allows for waypoints to be added, removed, and edited interactively by the user.
///     Waypoints are represented by <see cref="WaypointBeaconHudElement"/> elements in the GUI.
/// </remarks>
[HarmonyClientSidePatch]
public class WaypointBeaconsClientSystem : ClientSystem
{
    private readonly ClientMain _game;
    private readonly ICoreClientAPI _capi;

    private static WaypointBeaconsSettings _settings;
    private static readonly ConcurrentDictionary<string, WaypointBeaconHudElement> _waypointElements = [];

    /// <summary>
    ///     Initialises the waypoint beacons system, including registering hotkeys and configuring the system settings.
    /// </summary>
    /// <param name="game">The client main instance.</param>
    public WaypointBeaconsClientSystem(ClientMain game) : base(game)
    {
        G.Log.VerboseDebug("Starting waypoint beacon client system");

        _game = game;
        _capi = (ICoreClientAPI)game.Api;
        _settings = IOC.Services.GetRequiredService<WaypointBeaconsSettings>();

        _capi.Input.RegisterHotKey("EditWaypointFromBeacon", "Edit Selected Waypoint Beacon", GlKeys.U, HotkeyType.GUIOrOtherControls);
        _capi.Input.SetHotKeyHandler("EditWaypointFromBeacon", _ => _waypointElements.Values.TryInvokeFirst(p => p.IsAligned, p => p.OpenEditDialogue()));

        Update();
    }

    /// <summary>
    ///     Executes the system update logic during the game tick on a separate thread.
    /// </summary>
    /// <param name="dt">The delta time of the game tick.</param>
    public override void OnSeperateThreadGameTick(float dt) => Update();

    /// <summary>
    ///     Updates the state of the waypoint beacons, including opening, closing, or updating the dialogue 
    ///     for the waypoint elements.
    /// </summary>
    private void Update()
    {
        if (_capi.IsGamePaused) return;
        Repopulate();
        _waypointElements.Values.InvokeWhere(p => p.Closeable, p => _capi.Event.AwaitMainThreadTask(() => p.TryClose()));
        _waypointElements.Values.InvokeWhere(p => p.Openable, p => _capi.Event.AwaitMainThreadTask(() => p.TryOpen()));
    }

    /// <summary>
    ///     Synchronises the WaypointElements dictionary with the active waypoints.
    /// </summary>
    private static void Repopulate()
    {
        // Remove elements with null keys
        foreach (var kvp in _waypointElements)
        {
            if (kvp.Key is null)
            {
                kvp.Value.TryClose();
                kvp.Value.Dispose();
                _waypointElements.TryRemove(kvp);
            }
        }

        // Remove elements not in ActiveBeacons
        foreach (var kvp in _waypointElements)
        {
            if (_settings.ActiveBeacons.Contains(kvp.Key)) continue;
            kvp.Value.TryClose();
            kvp.Value.Dispose();
            _waypointElements.TryRemove(kvp.Key, out _);
        }

        // Add elements present in ActiveBeacons but missing in WaypointElements
        foreach (var id in _settings.ActiveBeacons.Where(id => id is not null && !_waypointElements.ContainsKey(id)))
        {
            ApiEx.Client.Event.AwaitMainThreadTask(() =>
            {
                var element = new WaypointBeaconHudElement(ApiEx.Client, id);
                _waypointElements.TryAdd(id, element);
            });
        }
    }

    /// <inheritdoc />
    public override EnumClientSystemType GetSystemType() => EnumClientSystemType.Render;

    /// <inheritdoc />
    public override string Name => nameof(WaypointBeacons);

    /// <summary>
    ///     Disposes of the floaty waypoint system, closing all waypoint elements and cleaning up resources.
    /// </summary>
    public void Dispose() => Dispose(_game);

    private static void ClearElements()
    {
        _waypointElements?.Values.InvokeForAll(p =>
        {
            p.TryClose();
            p.Dispose();
        });
        _waypointElements?.Clear();
    }

    /// <inheritdoc />
    public override void Dispose(ClientMain game)
    {
        if (game is not null) base.Dispose(game);
        ClearElements();
    }

    /// <summary>
    ///     Postfix patch for <see cref="WaypointMapLayer.OnDataFromServer"/>.
    ///     Updates the active waypoints list by removing waypoints that are no longer available.
    ///     Calls Repopulate if changes are made.
    /// </summary>
    /// <param name="data">
    ///     The raw byte array containing serialised waypoint data received from the server.
    /// </param>
    [HarmonyPostfix]
    [HarmonyPatch(typeof(WaypointMapLayer), nameof(WaypointMapLayer.OnDataFromServer))]
    public static void Harmony_WaypointMapLayer_OnDataFromServer_Postfix(List<Waypoint> ___ownWaypoints)
    {
        var waypointIds = ___ownWaypoints.Select(p => p.Guid);
        if (_settings.ActiveBeacons.RemoveAll(id => !waypointIds.Contains(id)) > 0)
        {
            ModSettings.World.Save(_settings);
        }
        Repopulate();

        foreach (var beacon in _waypointElements.Values)
        {
            var waypoint = ___ownWaypoints.FirstOrDefault(p => p.Guid == beacon.Waypoint.Guid);
            if (beacon.Waypoint.IsSameAs(waypoint)) continue;
            beacon.Rehydrate();
        }    
    }
}