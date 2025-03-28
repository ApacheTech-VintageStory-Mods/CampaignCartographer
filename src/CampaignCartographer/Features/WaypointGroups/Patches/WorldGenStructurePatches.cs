using Vintagestory.API.MathTools;
using Vintagestory.ServerMods;
using static OpenTK.Graphics.OpenGL.GL;

namespace ApacheTech.VintageMods.CampaignCartographer.Features.WaypointGroups.Patches;

[HarmonyPatchCategory(nameof(EnumAppSide.Server))]
public class WorldGenStructurePatches
{
    private static void Log(string messagePrefix, ICoreAPI api, BlockSchematicStructure schematic, Cuboidi location)
    {
        var fileName = schematic.FromFileName;

        // Ensure only structures from BetterRuins get logged.
        // The mod has all its structures within the "game" domain, not the "betterruins" domain, so this is much harder than it should be.
        var (assetLocation, asset) = api.Assets.AllAssets.FirstOrDefault(p => p.Key.GetName() == fileName);
        if (assetLocation is null) return;
        if (!asset.Origin.OriginPath.Contains("BetterRuins")) return;

        var message = $"[BetterRuins] {messagePrefix} - File: {assetLocation.ToShortString()} - Location: {location}";
        api.Logger.VerboseDebug(message);
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(WorldGenStructure), "TryGenerateRuinAtSurface")]
    public static void LogWhenRuinGeneratedOnSurface(bool __result, IWorldAccessor worldForCollectibleResolve,
        BlockSchematicStructure ___LastPlacedSchematic, Cuboidi ___LastPlacedSchematicLocation)
    {
        if (!__result) return;
        Log(messagePrefix: "Generated ruin on the surface", 
            api: worldForCollectibleResolve.Api, 
            schematic: ___LastPlacedSchematic, 
            location: ___LastPlacedSchematicLocation);
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(WorldGenStructure), "TryGenerateAtSurface")]
    public static void LogWhenStructureGeneratedOnSurface(bool __result, IWorldAccessor worldForCollectibleResolve,
        BlockSchematicStructure ___LastPlacedSchematic, Cuboidi ___LastPlacedSchematicLocation)
    {
        if (!__result) return;
        Log(messagePrefix: "Generated structure at the surface",
            api: worldForCollectibleResolve.Api,
            schematic: ___LastPlacedSchematic,
            location: ___LastPlacedSchematicLocation);
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(WorldGenStructure), "TryGenerateUnderwater")]
    public static void LogWhenStructureGeneratedUnderwater(bool __result, IWorldAccessor worldForCollectibleResolve,
        BlockSchematicStructure ___LastPlacedSchematic, Cuboidi ___LastPlacedSchematicLocation)
    {
        if (!__result) return;
        Log(messagePrefix: "Generated structure underwater",
            api: worldForCollectibleResolve.Api,
            schematic: ___LastPlacedSchematic,
            location: ___LastPlacedSchematicLocation);
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(WorldGenStructure), "TryGenerateUnderground")]
    public static void LogWhenStructureGeneratedUnderground(bool __result, IWorldAccessor worldForCollectibleResolve,
        BlockSchematicStructure ___LastPlacedSchematic, Cuboidi ___LastPlacedSchematicLocation)
    {
        if (!__result) return;
        Log(messagePrefix: "Generated structure underground",
            api: worldForCollectibleResolve.Api,
            schematic: ___LastPlacedSchematic,
            location: ___LastPlacedSchematicLocation);
    }
}

public class DebugRuinsLogger : ModSystem
{
    private ICoreAPI _api;
    private Harmony _harmony;

    public override void Start(ICoreAPI api)
    {
        _api = api;
        if (!Harmony.HasAnyPatches(Mod.Info.ModID))
        {
            _harmony = new Harmony(Mod.Info.ModID);
            _harmony.PatchCategory($"{api.Side}");
        }
    }

    public override void Dispose()
    {
        if (Harmony.HasAnyPatches(Mod.Info.ModID))
        {
            _harmony.UnpatchCategory($"{_api.Side}");
        }
    }
}