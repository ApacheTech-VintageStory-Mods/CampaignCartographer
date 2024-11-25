﻿namespace ApacheTech.VintageMods.CampaignCartographer.Features.Waypoints.Automatic.Patches;

public partial class AutoWaypointsPatches
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(GuiDialogTrader), "OnGuiOpened")]
    public static void Patch_GuiDialogTrader_OnGuiOpened_Prefix()
    {
        if (!Settings.Traders) return;
        ApiEx.ClientMain.EnqueueMainThreadTask(() =>
        {
            ApiEx.Client.TriggerChatMessage(".wpt");
        }, "");
    }
}