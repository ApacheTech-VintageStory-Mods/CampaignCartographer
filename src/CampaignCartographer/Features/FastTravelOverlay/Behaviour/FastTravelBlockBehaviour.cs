﻿using ApacheTech.Common.Extensions.Harmony;
using ApacheTech.VintageMods.CampaignCartographer.Features.FastTravelOverlay.Dialogue;
using ApacheTech.VintageMods.CampaignCartographer.Features.FastTravelOverlay.Models;
using ApacheTech.VintageMods.CampaignCartographer.Features.TeleporterService;
using Gantry.Services.FileSystem.Configuration;
using Vintagestory.API.MathTools;

// ReSharper disable StringLiteralTypo

namespace ApacheTech.VintageMods.CampaignCartographer.Features.FastTravelOverlay.Behaviour;

/// <summary>
///     Handles interactions and behaviours related to fast travel blocks, including managing the overlay node and handling interactions for teleportation and translocation.
/// </summary>
internal class FastTravelBlockBehaviour(Block block, FastTravelOverlaySettings settings) : BlockBehavior(block)
{
    private readonly FastTravelOverlaySettings _settings = settings;
    private readonly WorldInteraction _interaction = new()
    {
        HotKeyCode = "ctrl",
        MouseButton = EnumMouseButton.Right,
    };

    private readonly TeleporterClientService _clientService = IOC.Services.GetRequiredService<TeleporterClientService>();

    /// <inheritdoc />
    public override void OnBlockRemoved(IWorldAccessor world, BlockPos pos, ref EnumHandling handling)
    {
        RemoveOverlayNode(pos);
    }

    /// <inheritdoc />
    public override void OnBlockBroken(IWorldAccessor world, BlockPos pos, IPlayer byPlayer, ref EnumHandling handling)
    {
        RemoveOverlayNode(pos);
    }

    /// <inheritdoc />
    public override bool OnBlockInteractStart(IWorldAccessor world, IPlayer byPlayer, BlockSelection selection, ref EnumHandling handling)
    {
        if (!byPlayer.Entity.Controls.CtrlKey) return false;
        if (!ShouldHandle(world, selection, out var blockEntity)) return false;

        var node = _settings.Nodes.FirstOrDefault(p => p.Location.SourcePos == selection.Position);

        var dialogue = node is null
            ? FastTravelOverlayNodeDialogue.Create(GetOrCreateNode(selection, blockEntity))
            : FastTravelOverlayNodeDialogue.Edit(node);

        dialogue.ToggleGui();
        handling = EnumHandling.Handled;
        return true;
    }

    /// <inheritdoc />
    public override WorldInteraction[] GetPlacedBlockInteractionHelp(IWorldAccessor world, BlockSelection selection, IPlayer forPlayer, ref EnumHandling handling)
    {
        if (!ShouldHandle(world, selection, out _)) return [];
        var added = _settings.Nodes.Any(p => p.Location.SourcePos == selection.Position);
        var current = base.GetPlacedBlockInteractionHelp(world, selection, forPlayer, ref handling);
        return [_interaction.With(p => p.ActionLangCode = $"blockhelp-{(added ? "edit" : "add")}-fast-travel-node"), .. current];
    }

    /// <summary>
    ///     Determines whether the block at the given selection position should be handled based on its block entity type and conditions.
    /// </summary>
    /// <param name="world">The world accessor to interact with.</param>
    /// <param name="selection">The block selection for the block to check.</param>
    /// <param name="blockEntity">The block entity at the selection position.</param>
    /// <returns><c>true</c> if the block should be handled; otherwise, <c>false</c>.</returns>
    private static bool ShouldHandle(IWorldAccessor world, BlockSelection selection, out BlockEntity blockEntity)
    {
        blockEntity = world.BlockAccessor.GetBlockEntity(selection.Position);
        return blockEntity switch
        {
            BlockEntityStaticTranslocator translocator => translocator.FullyRepaired,
            BlockEntityTeleporter teleporter => teleporter.GetField<TeleporterLocation>("tpLocation")?.TargetName is not null,
            _ => false
        };
    }

    /// <summary>
    ///     Creates or retrieves an existing fast-travel overlay node for a given block selection and its block entity.
    /// </summary>
    /// <param name="selection">The block selection to create the node for.</param>
    /// <param name="blockEntity">The block entity that determines the node type and properties.</param>
    /// <returns>A new or existing fast travel overlay node.</returns>
    private FastTravelOverlayNode GetOrCreateNode(BlockSelection selection, BlockEntity blockEntity)
    {
        var node = new FastTravelOverlayNode();

        switch (blockEntity)
        {
            case BlockEntityStaticTranslocator translocator:
                node.Type = FastTravelBlockType.Translocator;
                node.Location.SourceName = FastTravelOverlay.T("DefaultTitle");
                node.Location.TargetPos = translocator.GetField<BlockPos>("tpLocation");
                node.NodeColour = _settings.TranslocatorColour;
                break;
            case BlockEntityTeleporter:
                node.Type = FastTravelBlockType.Teleporter;
                node.Location = _clientService.Teleporters.FirstOrDefault(p => p.SourcePos == selection.Position);
                node.NodeColour = _settings.TeleporterColour;
                break;
            default:
                node.Type = FastTravelBlockType.Unknown;
                node.NodeColour = _settings.ErrorColour;
                node.Location.SourceName = FastTravelOverlay.T("DefaultTitle");
                node.Location.TargetPos = selection.Position;
                node.Enabled = false;
                node.ShowPath = false;
                break;
        }

        node.Location!.SourcePos = selection.Position;
        return node;
    }

    /// <summary>
    ///     Removes the overlay node associated with the specified block position.
    /// </summary>
    /// <param name="blockPos">The position of the block to remove the overlay node for.</param>
    private void RemoveOverlayNode(BlockPos blockPos)
    {
        if (_settings.Nodes.RemoveAll(p => p.Location.SourcePos == blockPos) > 0)
        {
            ModSettings.World.Save(_settings);
        }
    }
}