using ApacheTech.VintageMods.CampaignCartographer.Domain.Extensions;
using Gantry.Core.Extensions.Api;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.MathTools;

// ReSharper disable InconsistentNaming
// ReSharper disable StringLiteralTypo

namespace ApacheTech.VintageMods.CampaignCartographer.Features.Waypoints.Automatic.Patches;

public partial class AutoWaypointsPatches
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(BlockStaticTranslocator), "OnEntityCollide")]
    public static void Patch_BlockStaticTranslocator_OnEntityCollide_Postfix(BlockStaticTranslocator __instance, ICoreClientAPI ___api, Entity entity, BlockPos pos)
    {
        if (___api.Side.IsServer()) return;
        if (++_timesRunBlock > 1) return;
        ___api.RegisterDelayedCallback(_ => _timesRunBlock = 0, 1000 * 3);
        if (!Settings.Translocators) return;
        if (entity != ___api.World.Player.Entity) return;
        __instance.ProcessWaypoints(pos);
    }
}