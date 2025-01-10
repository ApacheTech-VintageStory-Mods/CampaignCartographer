﻿using ApacheTech.VintageMods.CampaignCartographer.Features.TeleporterService.Behaviours;
using Gantry.Services.Network;

namespace ApacheTech.VintageMods.CampaignCartographer.Features.TeleporterService;

/// <summary>
///     Provides client-side functionality for managing teleporters, including handling network messages and modifying 
///     teleporter block behaviours.
/// </summary>
public class TeleporterClientService : ClientModSystem
{
    /// <summary>
    ///     Configures the client-side networking for the teleporter system, registering message types and handlers.
    /// </summary>
    /// <param name="api">The client API instance.</param>
    public override void StartClientSide(ICoreClientAPI api)
    {
        ApiEx.Logger.VerboseDebug("Starting teleporter service");
        IOC.Services
           .GetRequiredService<IClientNetworkService>()
           .ClientChannel(nameof(TeleporterManager))
           .RegisterMessageType<TpLocations>()
           .RegisterMessageType<TeleporterLocationsPacket>()
           .SetMessageHandler<TeleporterLocationsPacket>(OnLocationsReceived);
    }

    /// <summary>
    ///     Handles the receipt of teleporter location data from the server and updates the local list of teleporters.
    /// </summary>
    /// <param name="p">The packet containing teleporter location data.</param>
    private void OnLocationsReceived(TeleporterLocationsPacket p)
    {
        ApiEx.Logger.VerboseDebug("Teleporter locations updated.");
        Teleporters = p.Teleporters;
    }

    /// <summary>
    ///     Adds custom behaviours to teleporter blocks after assets have been finalised, ensuring they are properly 
    ///     configured for client-side functionality.
    /// </summary>
    /// <param name="api">The client API instance.</param>
    public override void AssetsFinalise(ICoreClientAPI api)
    {
        ApiEx.Logger.VerboseDebug("Adding teleporter behaviours to blocks.");
        api.World.Blocks.AddBehaviourToBlocks<BlockTeleporter, TeleporterBlockBehaviour>(b => new TeleporterBlockBehaviour(b));
    }

    /// <summary>
    ///     A list of known teleporter locations received from the server.
    /// </summary>
    public List<TeleporterLocation> Teleporters { get; set; }
}