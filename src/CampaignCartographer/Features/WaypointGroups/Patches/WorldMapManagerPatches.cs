using System.Threading;
using Gantry.Services.FileSystem.Configuration.Consumers;
using ApacheTech.Common.Extensions.Harmony;
using ApacheTech.VintageMods.CampaignCartographer.Features.WaypointGroups.Abstractions;
using Vintagestory.API.Util;

namespace ApacheTech.VintageMods.CampaignCartographer.Features.WaypointGroups.Patches;

[HarmonyClientSidePatch]
public class WorldMapManagerPatches : WorldSettingsConsumer<WaypointGroupsSettings>
{
    /// <summary>
    ///     Adds thread safety to the mapLayerGenThread.
    /// </summary>
    [HarmonyPrefix]
    [HarmonyPriority(Priority.First)]
    [HarmonyPatch(typeof(WorldMapManager), "OnLvlFinalize")]
    public static bool Harmony_WorldMapManager_OnLvlFinalize_Prefix(WorldMapManager __instance, ref Thread ___mapLayerGenThread, GuiDialogWorldMap ___worldMapDlg)
    {
        // In singleplayer, this patch also runs on the server. They use the same instance.
        var capi = ApiEx.Client;
        if (capi is null) return true;
        var mapAllowedClient = capi.World.Config.GetBool("allowMap", true) || capi.World.Player.Privileges.IndexOf("allowMap") != -1;
        if (mapAllowedClient)
        {
            ApiEx.Logger.VerboseDebug("Registering world map hotkeys");
            capi.Input.RegisterHotKey("worldmaphud", Lang.Get("Show/Hide Minimap"), GlKeys.F6, HotkeyType.HelpAndOverlays);
            capi.Input.RegisterHotKey("minimapposition", Lang.Get("keycontrol-minimap-position"), GlKeys.F6, HotkeyType.HelpAndOverlays, false, true, false);
            capi.Input.RegisterHotKey("worldmapdialog", Lang.Get("Show World Map"), GlKeys.M, HotkeyType.HelpAndOverlays);
            capi.Input.SetHotKeyHandler("worldmaphud", key => __instance.CallMethod<bool>("OnHotKeyWorldMapHud", key));
            capi.Input.SetHotKeyHandler("minimapposition", key => __instance.CallMethod<bool>("OnHotKeyMinimapPosition", key));
            capi.Input.SetHotKeyHandler("worldmapdialog", key => __instance.CallMethod<bool>("OnHotKeyWorldMapDlg", key));

            ApiEx.Logger.VerboseDebug("Registering world map link protocols");
            capi.RegisterLinkProtocol("worldmap", link => __instance.CallMethod("onWorldMapLinkClicked", link));
        }

        lock (MapLayerGeneration.MapLayersLock)
        {
            ApiEx.Logger.VerboseDebug("Creating map layer instances");
            foreach (var (key, value) in __instance.MapLayerRegistry)
            {
                if (key == "entities" && !capi.World.Config.GetAsBool("entityMapLayer")) continue;
                ApiEx.Logger.VerboseDebug($" - {key}: {value.Name}");
                var instance = ActivatorUtilities.CreateInstance(IOC.Services, value, __instance).To<MapLayer>();
                __instance.MapLayers.Add(instance);
                instance.OnLoaded();
            }
        }

        ApiEx.Logger.VerboseDebug("Creating map layer generation thread");
        ___mapLayerGenThread = new Thread(() =>
        {
            while (!__instance.IsShuttingDown)
            {
                MapLayerGeneration.ThreadControl.Wait();

                lock (MapLayerGeneration.MapLayersLock)
                {
                    foreach (var layer in __instance.MapLayers)
                    {
                        layer.OnOffThreadTick(20 / 1000f);
                    }
                }

                Thread.Sleep(20);
            }
        })
        {
            IsBackground = true
        };

        ApiEx.Logger.VerboseDebug("Starting map layer generation thread");
        ___mapLayerGenThread.Start();

        if (capi is not null && (capi.Settings.Bool["showMinimapHud"] || !capi.Settings.Bool.Exists("showMinimapHud"))
        && (___worldMapDlg is null || !___worldMapDlg.IsOpened()))
        {
            ApiEx.Logger.VerboseDebug("Opening mini-map");
            __instance.ToggleMap(EnumDialogType.HUD);
        }

        return false;
    }
}