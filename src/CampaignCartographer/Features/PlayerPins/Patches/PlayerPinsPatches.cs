#nullable enable
using System.Text;
using ApacheTech.VintageMods.CampaignCartographer.Features.PlayerPins.DataStructures;
using Gantry.Services.FileSystem.Configuration.Consumers;
using Vintagestory.Server;

// ReSharper disable StringLiteralTypo
// ReSharper disable IdentifierTypo

namespace ApacheTech.VintageMods.CampaignCartographer.Features.PlayerPins.Patches;

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
[HarmonyClientSidePatch]
public class PlayerPinsPatches : WorldSettingsConsumer<PlayerPinsSettings>
{
    private static ICoreClientAPI? _capi;
    private static PlayerPins? _system;
    private static IWorldMapManager? _mapSink;
    private static Dictionary<IPlayer, EntityMapComponent> PlayerPins { get; set; } = [];

    [HarmonyPostfix]
    [HarmonyPatch(typeof(PlayerMapLayer), MethodType.Constructor, typeof(ICoreAPI), typeof(IWorldMapManager))]
    public static void Patch_PlayerMapLayer_Constructor_Postfix(ICoreAPI api, IWorldMapManager mapsink)
    {
        if (api is ServerCoreAPI) return;
        _capi = (ICoreClientAPI)api;
        _system = _capi.ModLoader.GetModSystem<PlayerPins>();
        _mapSink = mapsink;
        PlayerPins = new Dictionary<IPlayer, EntityMapComponent>();
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(PlayerMapLayer), "OnLoaded")]
    public static bool Patch_PlayerMapLayer_Event_OnLoaded_Prefix()
    {
        if (_capi is null) return true;
        _capi.Event.LeaveWorld += DisposeComponents;
        _capi.Event.PlayerEntitySpawn += OnPlayerSpawn;
        _capi.Event.PlayerEntityDespawn += OnPlayerDespawn;
        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(PlayerMapLayer), "OnMapOpenedClient")]
    public static bool Patch_PlayerMapLayer_OnMapOpenedClient_Prefix()
    {
        if (_capi is null) return true;
        PlayerPins.PurgeValues();
        foreach (var player in _capi.World.AllOnlinePlayers)
        {
            if (player.Entity == null)
            {
                G.Logger.Warning("Can't add player {0} to world map, missing entity :<", player.PlayerUID);
            }
            else if (!_capi.World.Config.GetBool("mapHideOtherPlayers") || player.PlayerUID == _capi.World.Player.PlayerUID)
            {
                AddPlayerToMap(player);
            }
        }
        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(PlayerMapLayer), "Render")]
    public static bool Patch_PlayerMapLayer_Render_Prefix(PlayerMapLayer __instance, GuiElementMap mapElem, float dt)
    {
        if (_capi is null) return true;
        if (!__instance.Active) return true;
        foreach (var pin in PlayerPins)
        {
            if (mapElem.Api.ModLoader.IsModEnabled("th3rp") && !pin.Value.entity!.WatchedAttributes.GetBool("maptag", true)) continue;
            if (pin.Value.Texture.Disposed) continue;
            pin.Value.Render(mapElem, dt);
        }
        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(PlayerMapLayer), "OnMouseMoveClient")]
    public static bool Patch_PlayerMapLayer_OnMouseMoveClient_Prefix(MouseEvent args, GuiElementMap mapElem, StringBuilder hoverText)
    {
        if (_capi is null) return true;
        foreach (var pin in PlayerPins)
        {
            pin.Value.OnMouseMove(args, mapElem, hoverText);
        }
        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(PlayerMapLayer), "OnMouseUpClient")]
    public static bool Patch_PlayerMapLayer_OnMouseUpClient_Prefix(MouseEvent args, GuiElementMap mapElem)
    {
        if (_capi is null) return true;
        foreach (var pin in PlayerPins)
        {
            pin.Value.OnMouseUpOnElement(args, mapElem);
        }
        return false;
    }

    private static void OnPlayerDespawn(IPlayer player)
    {
        if (!PlayerPins.TryGetValue(player, out var entityMapComponent)) return;
        entityMapComponent.Dispose();
        PlayerPins.Remove(player);
    }

    private static void OnPlayerSpawn(IPlayer player)
    {
        if (_capi!.World.Config.GetBool("mapHideOtherPlayers") && player.PlayerUID != _capi.World.Player.PlayerUID) return;
        if (!_mapSink!.IsOpened || PlayerPins.ContainsKey(player)) return;
        AddPlayerToMap(player);
    }

    private static void AddPlayerToMap(IPlayer player)
    {
        if (_capi is null || _system is null || player.Entity is null) return;
        var textureType = player.PlayerUID == _capi.World.Player.PlayerUID
            ? PlayerRelation.Self
            : Settings!.HighlightedPlayers.ContainsValue(player.PlayerUID) ? PlayerRelation.Highlighted : PlayerRelation.Others;

        var texture = _system.TextureCache[textureType];
        var comp = new EntityMapComponent(_capi, texture, player.Entity);
        PlayerPins.Add(player, comp);
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(PlayerMapLayer), "Dispose")]
    public static bool Patch_PlayerMapLayer_Dispose_Prefix()
    {
        DisposeComponents();
        return true;
    }

    private static void DisposeComponents()
    {
        foreach (var comp in PlayerPins.Values)
        {
            comp.Texture.Dispose();
        }
        PlayerPins.PurgeValues();
    }
}