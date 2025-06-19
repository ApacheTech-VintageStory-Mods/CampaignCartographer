using ApacheTech.VintageMods.CampaignCartographer.Features.ManualWaypoints.Extensions;
using Gantry.Core.Extensions.Api;
using Gantry.Services.FileSystem.Configuration.Consumers;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.MathTools;

// ReSharper disable InconsistentNaming
// ReSharper disable StringLiteralTypo

namespace ApacheTech.VintageMods.CampaignCartographer.Features.AutoWaypoints.Patches;

/// <summary>
///     A class for patching various block and entity interactions to automatically handle waypoints.
/// </summary>
/// <remarks>
///     This class contains Harmony patches that are applied to various game mechanics related to block interactions,
///     such as block interaction, block destruction, teleporter collision, and static translocator interactions.
///     It automatically adds waypoints when conditions are met, for example, when a player interacts with or breaks a block,
///     or when they collide with a teleporter or translocator.
/// </remarks>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
[HarmonyClientSidePatch]
public class AutoWaypointsPatches : WorldSettingsConsumer<AutoWaypointsSettings>
{
    private static AutoWaypointPatchHandler? _handler;
    private static int _timesRunBlock;

    /// <summary>
    ///     Postfix patch for <see cref="Block.OnBlockInteractStart"/> that adds waypoints when a player interacts with a block.
    /// </summary>
    /// <param name="__instance">The block instance that was interacted with.</param>
    /// <remarks>
    ///     This method adds a waypoint when a player interacts with a block, provided the side is client-side and the
    ///     action has not been run more than once within the last second.
    /// </remarks>
    [HarmonyPostfix]
    [HarmonyPatch(typeof(Block), nameof(Block.OnBlockInteractStart))]
    public static void Patch_Block_OnBlockInteractStart_Postfix(Block __instance)
    {
        if (ApiEx.Side.IsServer()) return;
        if (++_timesRunBlock > 1) return;
        ApiEx.Client.RegisterDelayedCallback(_ => _timesRunBlock = 0, 1000);

        _handler ??= IOC.Services.GetRequiredService<AutoWaypointPatchHandler>();
        _handler.HandleInteraction(__instance);
    }

    /// <summary>
    ///     Postfix patch for <see cref="Block.OnBlockBroken"/> that adds waypoints when a player breaks a block.
    /// </summary>
    /// <param name="__instance">The block instance that was broken.</param>
    /// <remarks>
    ///     This method adds a waypoint when a player breaks a block, provided the side is client-side and the
    ///     action has not been run more than once within the last second.
    /// </remarks>
    [HarmonyPostfix]
    [HarmonyPatch(typeof(Block), nameof(Block.OnBlockBroken))]
    public static void Patch_Block_OnBlockBroken_Postfix(Block __instance)
    {
        if (ApiEx.Side.IsServer()) return;
        if (++_timesRunBlock > 1) return;
        ApiEx.Client.RegisterDelayedCallback(_ => _timesRunBlock = 0, 1000);

        _handler ??= IOC.Services.GetRequiredService<AutoWaypointPatchHandler>();
        _handler.HandleInteraction(__instance);
    }

    /// <summary>
    ///     Prefix patch for <see cref="GuiDialogTrader.OnGuiOpened"/> that triggers a waypoint-related chat message.
    /// </summary>
    /// <remarks>
    ///     This method triggers a waypoint-related chat message when a trader's GUI is opened, provided the setting is enabled.
    /// </remarks>
    [HarmonyPrefix]
    [HarmonyPatch(typeof(GuiDialogTrader), nameof(GuiDialogTrader.OnGuiOpened))]
    public static void Patch_GuiDialogTrader_OnGuiOpened_Prefix()
    {
        if (Settings is null) return;
        if (!Settings.Traders) return;
        ApiEx.ClientMain.EnqueueMainThreadTask(() =>
        {
            ApiEx.Client.TriggerChatMessage(".wpt");
        }, "");
    }

    /// <summary>
    ///     Prefix patch for <see cref="BlockStaticTranslocator.OnEntityCollide"/> that processes waypoints when an entity collides with a translocator.
    /// </summary>
    /// <param name="__instance">The translocator that the entity collided with.</param>
    /// <param name="___api">The API instance.</param>
    /// <param name="entity">The entity that collided with the translocator.</param>
    /// <param name="pos">The position of the translocator.</param>
    /// <remarks>
    ///     This method processes waypoints when an entity collides with a translocator, provided certain conditions are met,
    ///     such as the player being the entity and translocators being enabled in the settings.
    /// </remarks>
    [HarmonyPrefix]
    [HarmonyPatch(typeof(BlockStaticTranslocator), nameof(BlockStaticTranslocator.OnEntityCollide))]
    public static void Patch_BlockStaticTranslocator_OnEntityCollide_Postfix(BlockStaticTranslocator __instance, ICoreClientAPI ___api, Entity entity, BlockPos pos)
    {
        if (Settings is null) return;
        if (___api.Side.IsServer()) return;
        if (++_timesRunBlock > 1) return;
        ___api.RegisterDelayedCallback(_ => _timesRunBlock = 0, 1000 * 3);
        if (!Settings.Translocators) return;
        if (entity != ___api.World.Player.Entity) return;
        __instance.ProcessWaypoints(pos);
    }

    /// <summary>
    ///     Prefix patch for <see cref="BlockEntityTeleporterBase.OnEntityCollide"/> that adds a waypoint when a player collides with a teleporter.
    /// </summary>
    /// <param name="__instance">The instance of the teleporter entity.</param>
    /// <param name="___tpingEntities">The dictionary of entities currently teleporting.</param>
    /// <remarks>
    ///     This method adds a waypoint when a player collides with a teleporter, provided the player is teleporting
    ///     and the waypoint doesn't already exist.
    /// </remarks>
    [HarmonyPrefix]
    [HarmonyPatch(typeof(BlockEntityTeleporterBase), nameof(BlockEntityTeleporterBase.OnEntityCollide))]
    public static void Patch_BlockEntityTeleporter_OnEntityCollide_Prefix(BlockEntityTeleporterBase __instance, IReadOnlyDictionary<long, TeleportingEntity> ___tpingEntities)
    {
        if (Settings is null) return;
        if (ApiEx.Side.IsServer()) return;
        if (__instance is BlockEntityStaticTranslocator) return;
        if (!Settings.Teleporters) return;
        var playerId = ApiEx.Client.World.Player.Entity.EntityId;
        if (!___tpingEntities.ContainsKey(playerId)) return;
        if (___tpingEntities[playerId].Entity.Pos.AsBlockPos.WaypointExistsWithinRadius(1, 1)) return;

        var titleTemplate = LangEx.FeatureCode("PredefinedWaypoints.TeleporterWaypoints", "TeleporterWaypointTitle");
        __instance.AddWaypoint(titleTemplate);
    }
}