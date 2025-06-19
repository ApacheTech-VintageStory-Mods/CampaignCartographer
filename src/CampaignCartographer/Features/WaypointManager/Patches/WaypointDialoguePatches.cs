using System.Reflection.Emit;
using ApacheTech.Common.Extensions.Harmony;
using ApacheTech.VintageMods.CampaignCartographer.Features.WaypointManager.Dialogues;
using Gantry.Core.Extensions.Harmony;
using Gantry.Services.Network.Packets;
using Vintagestory.API.MathTools;
using System.Text;
using Gantry.Services.Network.Extensions;

// ReSharper disable InconsistentNaming

namespace ApacheTech.VintageMods.CampaignCartographer.Features.WaypointManager.Patches;

/// <summary>
///     Contains Harmony patches for the Waypoint Dialogue, enabling custom behaviour for waypoint editing and creation.
/// </summary>
[HarmonyClientSidePatch]
public static class WaypointDialoguePatches
{
    /// <summary>
    ///     Harmony transpiler for <see cref="GuiDialogWorldMap.OnMouseUp"/> to inject custom dialogue display logic.
    /// </summary>
    /// <param name="instructions">The original IL instructions.</param>
    /// <returns>The modified IL instructions with injected logic.</returns>
    [HarmonyTranspiler]
    [HarmonyPatch(typeof(GuiDialogWorldMap), nameof(GuiDialogWorldMap.OnMouseUp))]
    public static IEnumerable<CodeInstruction> Harmony_GuiDialogueWorldMap_OnMouseUp_Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        var targetCheck = AccessTools.PropertyGetter(typeof(MouseEvent), nameof(MouseEvent.Button));
        var showDialoguePatch = AccessTools.Method(typeof(WaypointDialoguePatches), nameof(ShowDialogue));

        var il = instructions.ToList();
        var statementIndex = il.FindIndex(p => p.Calls(targetCheck));
        var startIndex = il.FindIndex(statementIndex, p => p.opcode == OpCodes.Bne_Un);
        var endIndex = il.FindLabel(startIndex);

        return il.Inject(startIndex, endIndex, [
            new CodeInstruction(OpCodes.Ldarg_0),
            new CodeInstruction(OpCodes.Ldarg_1),
            new CodeInstruction(OpCodes.Call, showDialoguePatch)
        ]);
    }
    
    /// <summary>
    ///     Displays the Add/Edit Waypoint Dialogue at the specified world position.
    /// </summary>
    /// <param name="instance">The world map dialogue instance.</param>
    /// <param name="args">The mouse event arguments.</param>
    public static void ShowDialogue(GuiDialogWorldMap instance, MouseEvent args)
    {
        var wpPos = new Vec3d();
        LoadWorldPos(instance, args.X, args.Y, ref wpPos);
        var dialogue = new AddEditWaypointDialogue(ApiEx.Client, wpPos.AsBlockPos);
        dialogue.ToggleGui();
        dialogue.OnClosed += () => ApiEx.Client.Gui.RequestFocus(instance);
    }

    /// <summary>
    ///     Loads the world position from the mouse coordinates on the map.
    /// </summary>
    /// <param name="instance">The world map dialogue instance.</param>
    /// <param name="mouseX">The X coordinate of the mouse.</param>
    /// <param name="mouseY">The Y coordinate of the mouse.</param>
    /// <param name="worldPos">The output world position.</param>
    private static void LoadWorldPos(GuiDialogWorldMap instance, double mouseX, double mouseY, ref Vec3d worldPos)
    {
        var composer = instance.SingleComposer;
        var x = mouseX - composer.Bounds.absX;
        var y = mouseY - composer.Bounds.absY;
        if (instance.GetField<EnumDialogType>("dialogType") == EnumDialogType.Dialog) y -= GuiElement.scaled(30.0);
        composer.GetElement("mapElem").To<GuiElementMap>().TranslateViewPosToWorldPos(new Vec2f((float)x, (float)y), ref worldPos);
        worldPos.Y += 1.0;
    }

    /// <summary>
    ///     Harmony prefix for <see cref="WaypointMapComponent.OnMouseUpOnElement"/> to show the Add/Edit Waypoint Dialogue or teleport.
    /// </summary>
    /// <param name="__instance">The waypoint map component instance.</param>
    /// <param name="args">The mouse event arguments.</param>
    /// <param name="mapElem">The map element.</param>
    /// <param name="___waypoint">The waypoint being interacted with.</param>
    /// <param name="___waypointIndex">The index of the waypoint.</param>
    /// <returns>False if the event is handled; otherwise, true.</returns>
    [HarmonyPrefix]
    [HarmonyPatch(typeof(WaypointMapComponent), nameof(WaypointMapComponent.OnMouseUpOnElement))]
    public static bool Harmony_WaypointMapComponent_OnMouseUpOnElement_Prefix(
        WaypointMapComponent __instance,
        MouseEvent args,
        GuiElementMap mapElem,
        Waypoint ___waypoint,
        int ___waypointIndex)
    {
        if (args.Button != EnumMouseButton.Right) return true;
        var player = ApiEx.Client.World.Player;

        var viewPos = new Vec2f();
        mapElem.TranslateWorldPosToViewPos(___waypoint.Position, ref viewPos);
        var x = viewPos.X + mapElem.Bounds.renderX;
        var y = viewPos.Y + mapElem.Bounds.renderY;
        if (___waypoint.Pinned)
        {
            mapElem.ClampButPreserveAngle(ref viewPos, 2);
            x = viewPos.X + mapElem.Bounds.renderX;
            y = viewPos.Y + mapElem.Bounds.renderY;
            x = (float)GameMath.Clamp(x, mapElem.Bounds.renderX + 2.0, mapElem.Bounds.renderX + mapElem.Bounds.InnerWidth - 2.0);
            y = (float)GameMath.Clamp(y, mapElem.Bounds.renderY + 2.0, mapElem.Bounds.renderY + mapElem.Bounds.InnerHeight - 2.0);
        }
        var value = args.X - x;
        var dY = args.Y - y;
        var size = RuntimeEnv.GUIScale * 8f;
        if (!(Math.Abs(value) < size) || !(Math.Abs(dY) < size)) return false;
        if (player.Entity.Controls.ShiftKey && player.WorldData.CurrentGameMode == EnumGameMode.Creative)
        {
            ApiEx.Client.Network
                .GetDefaultChannel()?
                .SendPacket<WorldMapTeleportPacket>(new() { Position = ___waypoint.Position });
            args.Handled = true;
            return false;
        }

        var map = __instance.capi.ModLoader.GetModSystem<WorldMapManager>().worldMapDlg;
        var dialogue = new AddEditWaypointDialogue(ApiEx.Client, ___waypoint, ___waypointIndex);
        dialogue.ToggleGui();
        dialogue.OnClosed += () => __instance.capi.Gui.RequestFocus(map);
        args.Handled = true;
        return false;
    }
}

/// <summary>
///     Contains Harmony patches for the WaypointMapComponent, such as hover text enrichment.
/// </summary>
[HarmonyClientSidePatch]
public static class WaypointMapComponentPatches
{
    /// <summary>
    ///     Harmony postfix for <see cref="WaypointMapComponent.OnMouseMove"/> to append waypoint text to the hover text.
    /// </summary>
    /// <param name="hoverText">The hover text StringBuilder.</param>
    /// <param name="___waypoint">The waypoint being hovered over.</param>
    [HarmonyPostfix]
    [HarmonyPatch(typeof(WaypointMapComponent), nameof(WaypointMapComponent.OnMouseMove))]
    public static void Harmony_WaypointMapComponent_OnMouseMove_Postfix(StringBuilder hoverText, Waypoint ___waypoint)
    {
        if (string.IsNullOrEmpty(___waypoint.Text)) return;
        if (!hoverText.ToString().Contains(___waypoint.Title)) return;
        hoverText.AppendLine(___waypoint.Text);
    }
}