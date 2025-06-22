using Gantry.Services.FileSystem.Configuration.Consumers;
using ApacheTech.Common.Extensions.Harmony;
using ApacheTech.VintageMods.CampaignCartographer.Features.WaypointGroups.Abstractions;
using Vintagestory.API.Util;

namespace ApacheTech.VintageMods.CampaignCartographer.Features.WaypointGroups.Patches;

// In singleplayer, this patch also runs on the server. They use the same event hook.
// Also patch on server, so that it gets explicitly unpatched on dispose.

/// <summary>
///     Harmony patch for WorldMapManager to add thread safety and custom map layer handling.
///     This patch registers hotkeys, link protocols, creates map layer instances, starts the map layer generation thread,
///     and opens the minimap HUD if needed. The logic is split into helper methods for easier debugging and maintenance.
/// </summary>
[HarmonyUniversalPatch]
public class WorldMapManagerPatches : WorldSettingsConsumer<WaypointGroupsSettings>
{
    /// <summary>
    ///     Harmony prefix for WorldMapManager.OnLvlFinalize. Sets up hotkeys, link protocols, map layers, and minimap thread.
    ///     Returns false to skip the original method.
    /// </summary>
    /// <param name="__instance">The WorldMapManager instance.</param>
    /// <param name="___capi">The client API instance.</param>
    /// <param name="___mapLayerGenThread">Reference to the map layer generation thread.</param>
    /// <param name="___worldMapDlg">The world map dialog instance.</param>
    /// <returns>False to skip the original method, true to continue with the original.</returns>
    [HarmonyPrefix]
    [HarmonyPriority(Priority.First)]
    [HarmonyPatch(typeof(WorldMapManager), "OnLvlFinalize")]
    public static bool Harmony_WorldMapManager_OnLvlFinalize_Prefix(WorldMapManager __instance, ICoreClientAPI ___capi, ref Thread ___mapLayerGenThread, GuiDialogWorldMap ___worldMapDlg)
    {
        try
        {
            if (___capi is null) return true;
            G.Log("[WorldMapManagerPatches] Entering OnLvlFinalize prefix");
            var mapAllowedClient = ___capi.World.Config.GetBool("allowMap", true) || ___capi.World.Player.Privileges.IndexOf("allowMap") != -1;
            if (mapAllowedClient)
            {
                RegisterWorldMapHotkeys(__instance, ___capi);
                RegisterWorldMapLinkProtocols(__instance, ___capi);
            }

            CreateMapLayerInstances(__instance, ___capi);
            StartMapLayerGenerationThread(__instance, ref ___mapLayerGenThread);
            OpenMinimapIfNeeded(__instance, ___capi, ___worldMapDlg);
            G.Log("[WorldMapManagerPatches] Exiting OnLvlFinalize prefix");
            return false;
        }
        catch (Exception ex)
        {
            G.Logger.Error("[WorldMapManagerPatches] Exception in OnLvlFinalize prefix: " + ex);
            return true;
        }
    }

    /// <summary>
    ///     Registers hotkeys for world map and minimap controls.
    ///     Logs and handles any exceptions that occur during registration.
    /// </summary>
    /// <param name="instance">The WorldMapManager instance.</param>
    /// <param name="capi">The client API instance.</param>
    private static void RegisterWorldMapHotkeys(WorldMapManager instance, ICoreClientAPI capi)
    {
        try
        {
            G.Log("[WorldMapManagerPatches] Registering world map hotkeys");
            capi.Input.RegisterHotKey("worldmaphud", Lang.Get("Show/Hide Minimap"), GlKeys.F6, HotkeyType.HelpAndOverlays);
            capi.Input.RegisterHotKey("minimapposition", Lang.Get("keycontrol-minimap-position"), GlKeys.F6, HotkeyType.HelpAndOverlays, false, true, false);
            capi.Input.RegisterHotKey("worldmapdialog", Lang.Get("Show World Map"), GlKeys.M, HotkeyType.HelpAndOverlays);
            capi.Input.SetHotKeyHandler("worldmaphud", key => instance.CallMethod<bool>("OnHotKeyWorldMapHud", key));
            capi.Input.SetHotKeyHandler("minimapposition", key => instance.CallMethod<bool>("OnHotKeyMinimapPosition", key));
            capi.Input.SetHotKeyHandler("worldmapdialog", key => instance.CallMethod<bool>("OnHotKeyWorldMapDlg", key));
        }
        catch (Exception ex)
        {
            G.Logger.Error("[WorldMapManagerPatches] Exception in RegisterWorldMapHotkeys: " + ex);
            throw;
        }
    }

    /// <summary>
    ///     Registers the link protocol for world map links.
    ///     Logs and handles any exceptions that occur during registration.
    /// </summary>
    /// <param name="instance">The WorldMapManager instance.</param>
    /// <param name="capi">The client API instance.</param>
    private static void RegisterWorldMapLinkProtocols(WorldMapManager instance, ICoreClientAPI capi)
    {
        try
        {
            G.Log("[WorldMapManagerPatches] Registering world map link protocols");
            capi.RegisterLinkProtocol("worldmap", link => instance.CallMethod("onWorldMapLinkClicked", link));
        }
        catch (Exception ex)
        {
            G.Logger.Error("[WorldMapManagerPatches] Exception in RegisterWorldMapLinkProtocols: " + ex);
            throw;
        }
    }

    /// <summary>
    ///     Creates and loads all map layer instances from the registry.
    ///     Each layer is created, added, and loaded individually, with error handling and logging for each step.
    /// </summary>
    /// <param name="instance">The WorldMapManager instance.</param>
    /// <param name="capi">The client API instance.</param>
    private static void CreateMapLayerInstances(WorldMapManager instance, ICoreClientAPI capi)
    {
        try
        {
            lock (MapLayerGeneration.MapLayersLock)
            {
                G.Log("[WorldMapManagerPatches] Creating map layer instances");
                var registry = GetMapLayerRegistry(instance);
                foreach (var (key, value) in registry)
                {
                    try
                    {
                        if (!ShouldCreateMapLayer(key, capi)) continue;
                        G.Log($"[WorldMapManagerPatches] Creating map layer: {key}: {value.Name}");
                        AddMapLayer(instance, value);
                    }
                    catch (Exception exLayer)
                    {
                        G.Logger.Error($"[WorldMapManagerPatches] Exception creating map layer {key}: {exLayer}");
                        throw;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            G.Logger.Error("[WorldMapManagerPatches] Exception in CreateMapLayerInstances: " + ex);
            throw;
        }
    }

    /// <summary>
    ///     Gets a copy of the map layer registry from the WorldMapManager.
    /// </summary>
    /// <param name="instance">The WorldMapManager instance.</param>
    /// <returns>A dictionary of map layer keys and their corresponding types.</returns>
    private static Dictionary<string, Type> GetMapLayerRegistry(WorldMapManager instance)
        => new(instance.MapLayerRegistry);

    /// <summary>
    ///     Determines if a map layer should be created based on its key and the current configuration.
    /// </summary>
    /// <param name="key">The map layer key.</param>
    /// <param name="capi">The client API instance.</param>
    /// <returns>True if the map layer should be created, otherwise false.</returns>
    private static bool ShouldCreateMapLayer(string key, ICoreClientAPI capi)
        => key != "entities" || capi.World.Config.GetAsBool("entityMapLayer");

    /// <summary>
    ///     Creates a map layer instance using dependency injection.
    /// </summary>
    /// <param name="type">The type of the map layer.</param>
    /// <param name="instance">The WorldMapManager instance.</param>
    /// <returns>The created MapLayer instance.</returns>
    private static MapLayer CreateMapLayerInstance(Type type, WorldMapManager instance)
        => ActivatorUtilities.CreateInstance(IOC.Services, type, instance).To<MapLayer>();

    /// <summary>
    ///     Creates a map layer instance with retry logic for IOException.
    /// </summary>
    /// <param name="type">The type of the map layer.</param>
    /// <param name="instance">The WorldMapManager instance.</param>
    /// <param name="maxAttempts">The maximum number of retry attempts.</param>
    /// <param name="delayMs">The delay between retry attempts in milliseconds.</param>
    /// <returns>The created MapLayer instance.</returns>
    private static MapLayer CreateMapLayerWithRetry(Type type, WorldMapManager instance, int maxAttempts = 5, int delayMs = 200)
    {
        int attempt = 0;
        while (true)
        {
            try
            {
                return ActivatorUtilities.CreateInstance(IOC.Services, type, instance).To<MapLayer>();
            }
            catch (IOException ex) when (attempt < maxAttempts)
            {
                G.Logger.Error($"[WorldMapManagerPatches] Retrying {type.Name} construction due to IO error: {ex.Message}");
                Thread.Sleep(delayMs);
                attempt++;
            }
            catch (Exception ex)
            {
                G.Logger.Error($"[WorldMapManagerPatches] Exception constructing {type.Name}: {ex}");
                throw;
            }
        }
    }

    /// <summary>
    ///     Adds the map layer to the manager and calls OnLoaded. Logs and handles any exceptions.
    /// </summary>
    /// <param name="instance">The WorldMapManager instance.</param>
    /// <param name="type">The type of the map layer.</param>
    private static void AddMapLayer(WorldMapManager instance, Type type)
    {
        try
        {
            MapLayer mapLayer;
            mapLayer = CreateMapLayerWithRetry(type, instance);
            instance.MapLayers.Add(mapLayer);
            mapLayer.OnLoaded();
        }
        catch (Exception ex)
        {
            G.Logger.Error($"[WorldMapManagerPatches] Exception in AddMapLayer for {type.Name}: {ex}");
            throw;
        }
    }

    /// <summary>
    ///     Starts the background thread for map layer generation and ticking. Logs and handles any exceptions.
    /// </summary>
    /// <param name="instance">The WorldMapManager instance.</param>
    /// <param name="mapLayerGenThread">Reference to the map layer generation thread.</param>
    private static void StartMapLayerGenerationThread(WorldMapManager instance, ref Thread mapLayerGenThread)
    {
        try
        {
            G.Log("[WorldMapManagerPatches] Starting map layer generation thread");
            mapLayerGenThread = new Thread(() =>
            {
                try
                {
                    while (!instance.IsShuttingDown)
                    {
                        MapLayerGeneration.ThreadControl.Wait();
                        lock (MapLayerGeneration.MapLayersLock)
                        {
                            var layers = new List<MapLayer>(instance.MapLayers);
                            foreach (var layer in layers)
                            {
                                try
                                {
                                    layer.OnOffThreadTick(0.02f);
                                }
                                catch (Exception exLayer)
                                {
                                    G.Logger.Error($"[WorldMapManagerPatches] Exception in OnOffThreadTick for {layer.GetType().Name}: {exLayer}");
                                }
                            }
                        }
                        Thread.Sleep(20);
                    }
                }
                catch (Exception exThread)
                {
                    G.Logger.Error("[WorldMapManagerPatches] Exception in map layer generation thread: " + exThread);
                }
            })
            { IsBackground = true };
            mapLayerGenThread.Start();
        }
        catch (Exception ex)
        {
            G.Logger.Error("[WorldMapManagerPatches] Exception in StartMapLayerGenerationThread: " + ex);
        }
    }

    /// <summary>
    ///     Opens the minimap HUD if the settings and dialog state require it. Logs and handles any exceptions.
    /// </summary>
    /// <param name="instance">The WorldMapManager instance.</param>
    /// <param name="capi">The client API instance.</param>
    /// <param name="worldMapDlg">The world map dialog instance.</param>
    private static void OpenMinimapIfNeeded(WorldMapManager instance, ICoreClientAPI capi, GuiDialogWorldMap worldMapDlg)
    {
        try
        {
            if (capi is not null && (capi.Settings.Bool["showMinimapHud"] || !capi.Settings.Bool.Exists("showMinimapHud"))
                && (worldMapDlg is null || !worldMapDlg.IsOpened()))
            {
                G.Log("[WorldMapManagerPatches] Opening mini-map");
                instance.ToggleMap(EnumDialogType.HUD);
            }
        }
        catch (Exception ex)
        {
            G.Logger.Error("[WorldMapManagerPatches] Exception in OpenMinimapIfNeeded: " + ex);
        }
    }
}