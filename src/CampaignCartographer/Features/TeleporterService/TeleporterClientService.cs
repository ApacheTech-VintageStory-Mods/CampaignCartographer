using ApacheTech.VintageMods.CampaignCartographer.Features.TeleporterService.Behaviours;
using ApacheTech.VintageMods.CampaignCartographer.Features.TeleporterService.Dialogue;
using Gantry.Services.Network.Extensions;

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
        G.Log("Starting teleporter service");
        api.Network
            .GetOrRegisterDefaultChannel()
            .RegisterMessageHandler<TpLocations>(ShowDialogue)
            .RegisterMessageHandler<TeleporterLocationsPacket>(OnLocationsReceived);
    }

    /// <summary>
    ///     Displays the teleporter location dialogue when a packet is received.
    /// </summary>
    /// <param name="packet">The teleporter location data.</param>
    private void ShowDialogue(TpLocations packet)
    {
        var dialogue = new TeleporterLocationDialogue(ApiEx.Client, packet);
        dialogue.ToggleGui();
    }

    /// <summary>
    ///     Handles the receipt of teleporter location data from the server and updates the local list of teleporters.
    /// </summary>
    /// <param name="p">The packet containing teleporter location data.</param>
    private void OnLocationsReceived(TeleporterLocationsPacket p)
    {
        G.Log("Teleporter locations updated.");        
        TeleporterLocations = p.Teleporters;
    }

    /// <summary>
    ///     Adds custom behaviours to teleporter blocks after assets have been finalised, ensuring they are properly 
    ///     configured for client-side functionality.
    /// </summary>
    /// <remarks>
    ///     This fires before <see cref="StartClientSide"/>, so the network channel is not yet available."/>
    /// </remarks>
    /// <param name="api">The client API instance.</param>
    public override void AssetsFinalise(ICoreClientAPI api)
    {
        G.Log("Adding teleporter behaviours to blocks.");
        api.World.Blocks.AddBehaviourToBlocks<BlockTeleporter, TeleporterBlockBehaviour>(b => new TeleporterBlockBehaviour(b));
    }

    /// <summary>
    ///     A list of known teleporter locations received from the server.
    /// </summary>
    public List<TeleporterLocation> TeleporterLocations { get; set; }
}