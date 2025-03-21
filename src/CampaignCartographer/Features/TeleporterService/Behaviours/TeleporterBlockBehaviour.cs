using ApacheTech.VintageMods.CampaignCartographer.Features.FastTravelOverlay;
using ApacheTech.VintageMods.CampaignCartographer.Features.FastTravelOverlay.MapLayer;
using ApacheTech.VintageMods.CampaignCartographer.Features.TeleporterService.Dialogue;
using Gantry.Core.GameContent.Blocks;
using Gantry.Services.FileSystem.Configuration;
using Gantry.Services.Network.Extensions;
using Vintagestory.API.MathTools;

namespace ApacheTech.VintageMods.CampaignCartographer.Features.TeleporterService.Behaviours;

/// <summary>
///     Represents behaviour specific to teleporter blocks, including interactions and network communication.
/// </summary>
internal class TeleporterBlockBehaviour : BlockBehaviour<BlockTeleporter>
{
    private readonly IClientNetworkChannel _clientChannel;
    private readonly WorldInteraction _interaction = new()
    {
        HotKeyCode = "shift",
        MouseButton = EnumMouseButton.Right,
        ActionLangCode = "blockhelp-set-teleporter-location"
    };

    /// <summary>
    ///     Initialises a new instance of the <see cref="TeleporterBlockBehaviour" /> class.
    /// </summary>
    /// <param name="block">The block instance this behaviour is associated with.</param>
    public TeleporterBlockBehaviour(Block block) : base(block)
    {
        _clientChannel = ApiEx.Client.Network
            .GetDefaultChannel()
            .SetMessageHandler<TpLocations>(ShowDialogue);
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

    /// <inheritdoc />
    public override bool OnBlockInteractStart(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel, ref EnumHandling handling)
    {
        if (!byPlayer.Entity.Controls.ShiftKey)
            return base.OnBlockInteractStart(world, byPlayer, blockSel, ref handling);

        _clientChannel.SendPacket(new TpLocations { ForLocation = new TeleporterLocation { SourcePos = blockSel.Position } });
        handling = EnumHandling.Handled;
        return true;
    }

    /// <inheritdoc />
    public override WorldInteraction[] GetPlacedBlockInteractionHelp(IWorldAccessor world, BlockSelection selection, IPlayer forPlayer, ref EnumHandling handling)
    {
        var current = base.GetPlacedBlockInteractionHelp(world, selection, forPlayer, ref handling);
        return forPlayer.WorldData.CurrentGameMode == EnumGameMode.Creative
            ? [_interaction, .. current]
            : current;
    }

    /// <inheritdoc />
    public override void OnBlockBroken(IWorldAccessor world, BlockPos pos, IPlayer byPlayer, ref EnumHandling handling)
    {
        RemoveMapOverlayNode(pos);
        base.OnBlockBroken(world, pos, byPlayer, ref handling);
    }

    /// <inheritdoc />
    public override void OnBlockRemoved(IWorldAccessor world, BlockPos pos, ref EnumHandling handling)
    {
        RemoveMapOverlayNode(pos);
        base.OnBlockRemoved(world, pos, ref handling);
    }

    /// <summary>
    ///     Removes a map overlay node associated with a block position.
    /// </summary>
    /// <param name="pos">The position of the block being removed.</param>
    private static void RemoveMapOverlayNode(BlockPos pos)
    {
        var settings = IOC.Services.GetRequiredService<FastTravelOverlaySettings>();
        if (settings.Nodes.RemoveAll(p => p.Location.SourcePos == pos) <= 0) return;

        ModSettings.World.Save(settings);
        ApiEx.Client.GetMapLayer<FastTravelOverlayMapLayer>().RebuildMapComponents();
    }
}   