using System.Collections.Concurrent;
using ApacheTech.VintageMods.CampaignCartographer.Features.ModMenu.Extensions;
using ApacheTech.VintageMods.CampaignCartographer.Features.WaypointBeacons.Dialogue;
using ApacheTech.VintageMods.CampaignCartographer.Features.WaypointBeacons.Dialogue.Renderers;
using Gantry.Core.Extensions.DotNet;
using Gantry.Core.GameContent;
using Gantry.Core.Hosting.Registration;
using Gantry.Services.FileSystem.Configuration;
using Gantry.Services.FileSystem.Hosting;

namespace ApacheTech.VintageMods.CampaignCartographer.Features.WaypointBeacons;

/// <summary>
///     Represents the client-side mod system for managing waypoint beacons.
/// </summary>
[HarmonyClientSidePatch]
public class WaypointBeacons : ClientModSystem, IClientServiceRegistrar
{
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
        if (_settings?.ActiveBeacons.RemoveAll(id => !waypointIds.Contains(id)) > 0)
        {
            ModSettings.World.Save(_settings);
        }
        Repopulate();

        foreach (var beacon in _waypointElements.Values)
        {
            var waypoint = ___ownWaypoints.FirstOrDefault(p => p.Guid == beacon.Waypoint?.Guid);
            if (beacon.Waypoint?.IsSameAs(waypoint) is not true) continue;
            beacon.Rehydrate();
        }
    }

    private static WaypointBeaconsSettings? _settings;
    private long _listener;
    private static readonly ConcurrentDictionary<string, WaypointBeaconHudElement> _waypointElements = [];

    /// <summary>
    ///     Configures client-side services required by the waypoint beacons mod.
    /// </summary>
    /// <param name="services">The service collection to register dependencies into.</param>
    /// <param name="capi">The core client API instance.</param>
    public void ConfigureClientModServices(IServiceCollection services, ICoreClientAPI capi)
    {
        services.AddFeatureWorldSettings<WaypointBeaconsSettings>();
        services.AddSingleton<WaypointBeaconsSettingsDialogue>();
    }

    /// <inheritdoc />
    public override double ExecuteOrder() => 0.22;

    /// <inheritdoc />
    public override void StartClientSide(ICoreClientAPI capi)
    {
        G.Log("Starting waypoint beacons client mod system.");
        capi.AddModMenuDialogue<WaypointBeaconsSettingsDialogue>("WaypointBeacons");
        _settings = IOC.Services.GetRequiredService<WaypointBeaconsSettings>();

        Capi.Input.RegisterHotKey("EditWaypointFromBeacon", "Edit Selected Waypoint Beacon", GlKeys.U, HotkeyType.GUIOrOtherControls);
        Capi.Input.SetHotKeyHandler("EditWaypointFromBeacon", _ => _waypointElements.Values.TryInvokeFirst(p => p.IsAligned, p => p.OpenEditDialogue()));

        capi.Event.LevelFinalize += OnLevelFinalise;
    }

    /// <summary>
    ///     Handles the event when the level has been finalised, preparing waypoint beacons for rendering.
    /// </summary>
    private void OnLevelFinalise()
    {
        G.Log("Caching all waypoint icons for beacons");
        Capi.Event.LevelFinalize += () => WaypointIconFactory.PreCacheAllIcons(Capi);

        G.Log("Creating Waypoint Beacons render mesh flyweight");
        WaypointBeaconStore.Create();

        G.Log("Registering waypoint beacon update callback");
        _listener = Capi.Event.RegisterGameTickListener(Update, 20);
    }

    /// <summary>
    ///     Updates the state of the waypoint beacons, including opening, closing, or updating the dialogue 
    ///     for the waypoint elements.
    /// </summary>
    private void Update(float _)
    {
        if (Capi.IsGamePaused) return;
        Repopulate();
    }

    /// <summary>
    ///     Synchronises the WaypointElements dictionary with the active waypoints.
    /// </summary>
    private static void Repopulate()
    {
        _waypointElements.Values.InvokeWhere(p => p.Closeable, p => p.TryClose());
        _waypointElements.Values.InvokeWhere(p => p.Openable, p => p.TryOpen());

        // Remove elements with null keys
        foreach (var kvp in _waypointElements)
        {
            if (kvp.Key is null)
            {
                G.Log($"Removing beacon because key is null: {kvp.Value.Waypoint?.Guid}");
                kvp.Value.TryClose();
                kvp.Value.Dispose();
                _waypointElements.TryRemove(kvp);
            }
        }

        // Remove elements not in ActiveBeacons
        foreach (var kvp in _waypointElements)
        {
            if (_settings is null) continue;
            if (kvp.Key is null || kvp.Value.Waypoint is null) continue;
            if (_settings.ActiveBeacons.Contains(kvp.Key)) continue;
            G.Log($"Removing beacon because it is no longer active: {kvp.Value.Waypoint?.Guid}");
            kvp.Value.TryClose();
            kvp.Value.Dispose();
            _waypointElements.TryRemove(kvp.Key, out _);
        }

        // Add elements present in ActiveBeacons but missing in WaypointElements
        var elements = _settings?.ActiveBeacons.Where(id => id is not null && !_waypointElements.ContainsKey(id)) ?? [];
        foreach (var id in elements)
        {
            G.Log($"Adding beacon: {id}");
            var element = new WaypointBeaconHudElement(ApiEx.Client, id);
            _waypointElements.TryAdd(id, element);
        }
    }

    private static void ClearElements()
    {
        _waypointElements?.Values.InvokeForAll(p =>
        {
            p.TryClose();
            p.Dispose();
        });
        _waypointElements?.Clear();
    }

    /// <inheritdoc cref="ModSystem.Dispose" />
    public override void Dispose()
    {
        Capi.Event.LevelFinalize -= OnLevelFinalise;
        ClearElements();
        Capi.Event.UnregisterGameTickListener(_listener);
        WaypointIconFactory.Dispose();
        WaypointBeaconStore.Dispose();
    }
}