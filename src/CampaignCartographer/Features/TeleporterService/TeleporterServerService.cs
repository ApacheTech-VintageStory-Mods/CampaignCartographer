using ApacheTech.Common.Extensions.Harmony;
using Gantry.Services.Network.Extensions;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;

namespace ApacheTech.VintageMods.CampaignCartographer.Features.TeleporterService;

/// <summary>
///     Provides server-side functionality for managing teleporters, including synchronising teleporter data with clients 
///     and handling teleporter-related network messages.
/// </summary>
[HarmonyServerSidePatch]
internal class TeleporterServerService : ServerModSystem
{
    private static IServerNetworkChannel _serverChannel;
    private static TeleporterManager _vanillaSystem;

    /// <summary>
    ///     Configures the server-side networking for the teleporter system, registers message types and handlers, 
    ///     and sets up event listeners.
    /// </summary>
    /// <param name="api">The server API instance.</param>
    public override void StartServerSide(ICoreServerAPI api)
    {
        G.Log("Starting teleporter service");
        _vanillaSystem = Sapi.ModLoader.GetModSystem<TeleporterManager>();

        _serverChannel = api.Network
            .GetOrRegisterDefaultChannel()
            .RegisterMessageHandler<TpLocations>(SendDataToClient)
            .RegisterMessageType<TeleporterLocationsPacket>();

        api.Event.PlayerJoin += SendLocationsToPlayer;
    }

    /// <summary>
    ///     Sends the current teleporter locations to a specific player when they join the server.
    /// </summary>
    /// <param name="byPlayer">The player who has joined the server.</param>
    private static void SendLocationsToPlayer(IServerPlayer byPlayer)
    {
        var locations = _vanillaSystem.GetField<Dictionary<BlockPos, TeleporterLocation>>("Locations");
        _serverChannel.SendPacket(new TeleporterLocationsPacket
        {
            Teleporters = [.. locations.Values]
        }, byPlayer);
    }

    /// <summary>
    ///     Sends teleporter location data to a client in response to a specific request.
    /// </summary>
    /// <param name="fromPlayer">The player who sent the request.</param>
    /// <param name="packet">The packet containing the request details.</param>
    private void SendDataToClient(IServerPlayer fromPlayer, TpLocations packet)
    {
        var forLocation = _vanillaSystem.CallMethod<TeleporterLocation>("GetOrCreateLocation", packet.ForLocation.SourcePos);
        var locations = _vanillaSystem.GetField<Dictionary<BlockPos, TeleporterLocation>>("Locations");
        _serverChannel.SendPacket(new TpLocations
        {
            ForLocation = forLocation,
            Locations = locations
        }, fromPlayer);
    }

    /// <summary>
    ///     Synchronises teleporter locations with the client after a location is set on the server.
    /// </summary>
    /// <param name="fromPlayer">The player who set the teleporter location.</param>
    [HarmonyPostfix]
    [HarmonyPatch(typeof(TeleporterManager), "OnSetLocationReceived")]
    public static void Harmony_TeleporterManager_OnSetLocationReceived_Postfix(IServerPlayer fromPlayer)
    {
        SendLocationsToPlayer(fromPlayer);
    }
}