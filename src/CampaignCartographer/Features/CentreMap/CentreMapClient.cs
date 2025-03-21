using ApacheTech.Common.Extensions.Harmony;
using ApacheTech.VintageMods.CampaignCartographer.Features.CentreMap.Packets;
using Gantry.Core.GameContent.ChatCommands.Parsers;
using Gantry.Core.GameContent.ChatCommands.Parsers.Extensions;
using Gantry.Services.Network.Extensions;
using Vintagestory.API.MathTools;

namespace ApacheTech.VintageMods.CampaignCartographer.Features.CentreMap;

/// <summary>
///     Feature: Centre Map
///      • Centre map on self.
///      • Centre map on an online player.
///      • Centre map on a specific X,Z location within the world.
///      • Centre map on a specific waypoint.
///      • Centre map on World Spawn.
///      • Centre map on the player's own spawn point.
/// </summary>
/// <seealso cref="ClientModSystem" />
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public sealed class CentreMapClient : ClientModSystem
{
    private ICoreClientAPI _capi;
    private WorldMapManager _worldMap;
    private IClientNetworkChannel _clientChannel;

    /// <summary>
    ///     Minor convenience method to save yourself the check for/cast to ICoreClientAPI in Start()
    /// </summary>
    /// <param name="capi">The core API implemented by the client. The main interface for accessing the client. Contains all sub-components, and some miscellaneous methods.</param>
    public override void StartClientSide(ICoreClientAPI capi)
    {
        G.Log("Starting centre-map service");
        _capi = capi;
        _worldMap = capi.ModLoader.GetModSystem<WorldMapManager>();

        _capi.Event.IsPlayerReady += Event_IsPlayerReady;

        _clientChannel = _capi.Network
            .GetOrRegisterDefaultChannel()
            .RegisterMessageType<PlayerSpawnPositionDto>()
            .SetMessageHandler<PlayerSpawnPositionDto>(OnClientSpawnPointResponsePacketReceived);

        CreateChatCommand();
    }

    private bool Event_IsPlayerReady(ref EnumHandling handling)
    {
        _capi.Event.PlayerEntitySpawn += OnPlayerSpawn;
        _capi.Event.PlayerEntityDespawn += OnPlayerDespawn;
        handling = EnumHandling.PassThrough;
        return true;
    }

    public override void Dispose()
    {
        _capi.Event.IsPlayerReady -= Event_IsPlayerReady;
        _capi.Event.PlayerEntitySpawn -= OnPlayerSpawn;
        _capi.Event.PlayerEntityDespawn -= OnPlayerDespawn;
    }

    private void CreateChatCommand()
    {
        var parsers = _capi.ChatCommands.Parsers;
        var cm = _capi.ChatCommands
            .Create("cm")
            .WithDescription(LangEx.FeatureString("CentreMap", "SettingsCommandDescription"))
            .HandleWith(OnSelfOption);

        cm.BeginSubCommand("self")
            .WithDescription(LangEx.FeatureString("CentreMap", "Self.Description"))
            .HandleWith(OnSelfOption)
            .EndSubCommand();

        cm.BeginSubCommand("home")
            .WithDescription(LangEx.FeatureString("CentreMap", "Home.Description"))
            .HandleWith(OnHomeOption)
            .EndSubCommand();

        cm.BeginSubCommand("player")
            .WithDescription(LangEx.FeatureString("CentreMap", "Player.Description"))
            .WithArgs(parsers.ClientPlayer())
            .HandleWith(OnPlayerOption)
            .EndSubCommand();

        cm.BeginSubCommand("pos")
            .WithDescription(LangEx.FeatureString("CentreMap", "Position.Description"))
            .WithArgs(parsers.WorldPosition2D("position"))
            .HandleWith(OnPositionOption)
            .EndSubCommand();

        cm.BeginSubCommand("spawn")
            .WithDescription(LangEx.FeatureString("CentreMap", "Spawn.Description"))
            .HandleWith(OnSpawnOption)
            .EndSubCommand();

        cm.BeginSubCommand("waypoint")
            .WithDescription(LangEx.FeatureString("CentreMap", "Waypoint.Description"))
            .WithArgs(parsers.Int("id"))
            .HandleWith(OnWaypointOption)
            .EndSubCommand();
    }

    private void OnPlayerSpawn(IClientPlayer byPlayer)
    {
        _capi.Event.EnqueueMainThreadTask(() =>
            _capi.Event.RegisterCallback(_ =>
                _worldMap.GetField<IClientNetworkChannel>("clientChannel")
                    .SendPacket(new OnViewChangedPacket()), 500), "");
    }

    private void OnPlayerDespawn(IClientPlayer byPlayer)
    {
        _capi.Event.EnqueueMainThreadTask(() =>
            _capi.Event.RegisterCallback(_ =>
                _worldMap.GetField<IClientNetworkChannel>("clientChannel")
                    .SendPacket(new OnViewChangedPacket()), 500), "");
    }

    #region Command Handlers

    /// <summary>
    ///      • Centre map on self.
    /// </summary>
    private TextCommandResult OnSelfOption(TextCommandCallingArgs args)
    {
        var player = args.Caller.Player;
        var displayPos = player.Entity.Pos.AsBlockPos.RelativeToSpawn();
        var message = LangEx.FeatureString("CentreMap", "CentreMapOnPlayer", player.PlayerName, displayPos.X,
            displayPos.Y, displayPos.Z);
        return RecentreAndProvideFeedback(player.Entity.Pos.XYZ, message);
    }

    /// <summary>
    ///      • Centre map on the player's own spawn point.
    /// </summary>
    private TextCommandResult OnHomeOption(TextCommandCallingArgs args)
    {
        if (!_clientChannel.Connected)
        {
            return TextCommandResult.Error(LangEx.Get("error-messages.mod-not-installed-on-server"));
        }

        _clientChannel.SendPacket<PlayerSpawnPositionDto>();
        return TextCommandResult.Success();
    }

    /// <summary>
    ///      • Centre map on an online player.
    /// </summary>
    private TextCommandResult OnPlayerOption(TextCommandCallingArgs args)
    {
        var parser = args.Parsers[0].To<GantryOnlinePlayersArgParser>();
        var searchTerm = parser.SearchTerm;
        var result = parser.GetValue();
        if (result is null)
        {
            return TextCommandResult.Error(LangEx.FeatureString("FuzzyPlayerSearch", "NoResults", searchTerm));
        }
        var player = result.To<IPlayer>();
        var displayPos = player.Entity.Pos.AsBlockPos.RelativeToSpawn();
        var message = LangEx.FeatureString("CentreMap", "CentreMapOnPlayer", player.PlayerName, displayPos.X,
            displayPos.Y, displayPos.Z);
        return RecentreAndProvideFeedback(player.Entity.Pos.XYZ, message);
    }

    /// <summary>
    ///      • Centre map on a specific X,Z location within the world.
    /// </summary>
    private TextCommandResult OnPositionOption(TextCommandCallingArgs args)
    {
        var position = args.Parsers[0].GetValue().To<Vec2i>();
        var pos = new BlockPos(position.X, 1, position.Y, Dimensions.NormalWorld).RelativeToSpawn();
        var message = LangEx.FeatureString("CentreMap", "CentreMapOnPosition", pos.X, pos.Z);
        return RecentreAndProvideFeedback(new Vec3d(position.X, 0, position.Y), message);
    }

    /// <summary>
    ///      • Centre map on World Spawn.
    /// </summary>
    private TextCommandResult OnSpawnOption(TextCommandCallingArgs args)
    {
        var pos = _capi.World.DefaultSpawnPosition.AsBlockPos;
        var displayPos = pos.RelativeToSpawn();
        var message = LangEx.FeatureString("CentreMap", "CentreMapOnWorldSpawn", displayPos.X, displayPos.Z);
        return RecentreAndProvideFeedback(pos.ToVec3d(), message);
    }

    /// <summary>
    ///      • Centre map on a specific waypoint.
    /// </summary>
    private TextCommandResult OnWaypointOption(TextCommandCallingArgs args)
    {
        var waypointId = args.Parsers[0].GetValue().To<int>();
        var target = _worldMap.WaypointMapLayer().ownWaypoints[waypointId];
        var pos = target.Position.AsBlockPos;
        var displayPos = pos.RelativeToSpawn();
        var message = LangEx.FeatureString("CentreMap", "CentreMapOnWaypoint", waypointId, target.Title,
            displayPos.X, displayPos.Z);
        return RecentreAndProvideFeedback(pos.ToVec3d(), message);
    }

    #endregion

    /// <summary>
    ///     Called when on the client, when the server sends a <see cref="PlayerSpawnPositionDto"/> packet.
    /// </summary>
    /// <param name="packet">The packet that was sent.</param>
    private void OnClientSpawnPointResponsePacketReceived(PlayerSpawnPositionDto packet)
    {
        var displayPos = packet.SpawnPosition.RelativeToSpawn();
        var message = LangEx.FeatureString("CentreMap", "CentreMapOnPlayerSpawn", displayPos.X, displayPos.Z);
        RecentreAndProvideFeedback(packet.SpawnPosition.ToVec3d(), message);
    }

    private TextCommandResult RecentreAndProvideFeedback(Vec3d position, string message)
    {
        _worldMap.RecentreMap(position);
        _capi.ShowChatMessage(message);
        return TextCommandResult.Success();
    }
}