﻿using System.Threading;
using Gantry.Services.FileSystem.Configuration.Consumers;
using ApacheTech.Common.Extensions.Harmony;
using ApacheTech.VintageMods.CampaignCartographer.Features.WaypointGroups.Abstractions;
using Vintagestory.API.Util;

namespace ApacheTech.VintageMods.CampaignCartographer.Features.WaypointGroups.Patches;

// In singleplayer, this patch also runs on the server. They use the same event hook.
// Also patch on server, so that it gets explicitly unpatched on dispose.
[HarmonyUniversalPatch]
public class WorldMapManagerPatches : WorldSettingsConsumer<WaypointGroupsSettings>
{
    /// <summary>
    ///     Adds thread safety to the mapLayerGenThread.
    /// </summary>
    [HarmonyPrefix]
    [HarmonyPriority(Priority.First)]
    [HarmonyPatch(typeof(WorldMapManager), "OnLvlFinalize")]
    public static bool Harmony_WorldMapManager_OnLvlFinalize_Prefix(WorldMapManager __instance, ICoreClientAPI ___capi, ref Thread ___mapLayerGenThread, GuiDialogWorldMap ___worldMapDlg)
    {
        if (___capi is null) return true;
        var mapAllowedClient = ___capi.World.Config.GetBool("allowMap", true) || ___capi.World.Player.Privileges.IndexOf("allowMap") != -1;
        if (mapAllowedClient)
        {
            G.Log("Registering world map hotkeys");
            ___capi.Input.RegisterHotKey("worldmaphud", Lang.Get("Show/Hide Minimap"), GlKeys.F6, HotkeyType.HelpAndOverlays);
            ___capi.Input.RegisterHotKey("minimapposition", Lang.Get("keycontrol-minimap-position"), GlKeys.F6, HotkeyType.HelpAndOverlays, false, true, false);
            ___capi.Input.RegisterHotKey("worldmapdialog", Lang.Get("Show World Map"), GlKeys.M, HotkeyType.HelpAndOverlays);
            ___capi.Input.SetHotKeyHandler("worldmaphud", key => __instance.CallMethod<bool>("OnHotKeyWorldMapHud", key));
            ___capi.Input.SetHotKeyHandler("minimapposition", key => __instance.CallMethod<bool>("OnHotKeyMinimapPosition", key));
            ___capi.Input.SetHotKeyHandler("worldmapdialog", key => __instance.CallMethod<bool>("OnHotKeyWorldMapDlg", key));

            G.Log("Registering world map link protocols");
            ___capi.RegisterLinkProtocol("worldmap", link => __instance.CallMethod("onWorldMapLinkClicked", link));
        }

        lock (MapLayerGeneration.MapLayersLock)
        {
            G.Log("Creating map layer instances");
            var registry = new Dictionary<string, Type>(__instance.MapLayerRegistry);
            foreach (var (key, value) in registry)
            {
                if (key == "entities" && !___capi.World.Config.GetAsBool("entityMapLayer")) continue;
                G.Log($" - {key}: {value.Name}");
                var instance = ActivatorUtilities.CreateInstance(IOC.Services, value, __instance).To<MapLayer>();
                __instance.MapLayers.Add(instance);
                instance.OnLoaded();
            }
        }

        G.Log("Starting map layer generation thread");
        ___mapLayerGenThread = new Thread(() =>
        {
            while (!__instance.IsShuttingDown)
            {
                MapLayerGeneration.ThreadControl.Wait();

                lock (MapLayerGeneration.MapLayersLock)
                {
                    var layers = new List<MapLayer>(__instance.MapLayers);
                    foreach (var layer in layers)
                    {
                        layer.OnOffThreadTick(0.02f);
                    }
                }

                Thread.Sleep(20);
            }
        })
        { IsBackground = true };
        ___mapLayerGenThread.Start();

        if (___capi is not null && (___capi.Settings.Bool["showMinimapHud"] || !___capi.Settings.Bool.Exists("showMinimapHud"))
        && (___worldMapDlg is null || !___worldMapDlg.IsOpened()))
        {
            G.Log("Opening mini-map");
            __instance.ToggleMap(EnumDialogType.HUD);
        }

        return false;
    }
}