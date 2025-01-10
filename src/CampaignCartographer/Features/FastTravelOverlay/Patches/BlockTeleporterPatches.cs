using System.Reflection.Emit;
using Gantry.Core.Extensions.Harmony;

namespace ApacheTech.VintageMods.CampaignCartographer.Features.FastTravelOverlay.Patches;

/// <summary>
///     Client-side Harmony patches for modifying teleporter block behaviour.
/// </summary>
[HarmonyClientSidePatch]
internal class BlockTeleporterPatches
{
    /// <summary>
    ///     Transpiler for <see cref="BlockTeleporter.OnBlockInteractStart"/>.
    ///     Modifies the method to include an additional control key check during interaction.
    /// </summary>
    /// <param name="instructions">The original IL instructions to transpile.</param>
    /// <returns>
    ///     A modified sequence of <see cref="CodeInstruction"/> that includes the additional control key logic.
    /// </returns>
    [HarmonyTranspiler]
    [HarmonyPatch(typeof(BlockTeleporter), nameof(BlockTeleporter.OnBlockInteractStart))]
    internal static IEnumerable<CodeInstruction> Harmony_BlockTeleporter_OnBlockInteractStart_Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        var il = instructions.ToList();
        var startIndex = il.FindIndex(0, p => p.opcode == OpCodes.Bne_Un_S);
        var labelIndex = il.FindLabel(startIndex);
        return il.Skip(labelIndex);
    }
}
