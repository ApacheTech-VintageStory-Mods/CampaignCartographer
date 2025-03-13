using ApacheTech.VintageMods.CampaignCartographer.Features.WaypointManager.Dialogues.Exports;
using ApacheTech.VintageMods.CampaignCartographer.Features.WaypointManager.Dialogues.Imports;
using ApacheTech.VintageMods.CampaignCartographer.Features.ModMenu.Extensions;
using Gantry.Core.Hosting.Registration;
using Gantry.Core.GameContent.AssetEnum;
using ApacheTech.VintageMods.CampaignCartographer.Features.WaypointManager.Repositories;
using ApacheTech.VintageMods.CampaignCartographer.Features.WaypointManager.WaypointTemplates;
using Gantry.Core.Maths;
using Vintagestory.API.Server;
using Gantry.Core.Annotation;
using Gantry.Services.Network.Extensions;
using Gantry.Services.Network.Packets;
using Gantry.Core.GameContent.GUI.Abstractions;
using System.Diagnostics;
using ApacheTech.Common.Extensions.Harmony;

namespace ApacheTech.VintageMods.CampaignCartographer.Features.WaypointManager;

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
[HarmonyClientSidePatch]
public sealed class WaypointManager : UniversalModSystem, IClientServiceRegistrar
{
    public override double ExecuteOrder() => 0.11;

    public override void StartServerSide(ICoreServerAPI api)
    {
        api.Network.RegisterChannel(nameof(WaypointManager))
            .RegisterMessageHandler<WorldMapTeleportPacket>(OnTeleportPacketReceived)
            .RegisterMessageHandler<WaypointActionPacket>(OnWaypointPacketReceived);
    }

    [ServerSide]
    private void OnTeleportPacketReceived(IServerPlayer fromPlayer, WorldMapTeleportPacket packet)
    {
        fromPlayer.Entity.TeleportTo(packet.Position);
    }

    [ServerSide]
    private void OnWaypointPacketReceived(IServerPlayer fromPlayer, WaypointActionPacket packet)
    {
        switch (packet.Mode)
        {
            case AddEditDialogueMode.Add:
                AddWaypoint(packet.Waypoint, fromPlayer);
                break;
            case AddEditDialogueMode.Edit:
                EditWaypoint(packet.Waypoint, fromPlayer);
                break;
            default:
                throw new UnreachableException();
        }
    }

    [ServerSide]
    private void AddWaypoint(Waypoint waypoint, IServerPlayer player)
    {
        var mapManager = Sapi.ModLoader.GetModSystem<WorldMapManager>();
        var waypointMapLayer = mapManager.WaypointMapLayer();
        var wpIndex = waypointMapLayer.AddWaypoint(waypoint, player);

        var message = Lang.Get("Ok, waypoint nr. {0} added", wpIndex);
        Sapi.SendMessage(player, GlobalConstants.GeneralChatGroup, message, EnumChatType.CommandSuccess);
    }

    [ServerSide]
    private void EditWaypoint(Waypoint waypoint, IServerPlayer player)
    {
        var mapManager = Sapi.ModLoader.GetModSystem<WorldMapManager>();
        var waypointMapLayer = mapManager.WaypointMapLayer();

        var result = waypointMapLayer.Waypoints
            .Select((wp, index) => new { Waypoint = wp, Index = index })
            .Where(x => x.Waypoint.OwningPlayerUid == player?.PlayerUID)
            .SingleOrDefault(x => x.Waypoint.Guid == waypoint.Guid);

        if (result is null) return;

        var wpIndex = result.Index;
        var target = result.Waypoint;

        target.Position = waypoint.Position;
        target.Title = waypoint.Title;
        target.Text = waypoint.Text;
        target.Color = waypoint.Color;
        target.Icon = waypoint.Icon;
        target.ShowInWorld = waypoint.ShowInWorld;
        target.Pinned = waypoint.Pinned;

        waypointMapLayer.CallMethod("ResendWaypoints", player);

        var message = Lang.Get("Ok, waypoint nr. {0} modified", wpIndex);
        Sapi.SendMessage(player, GlobalConstants.GeneralChatGroup, message, EnumChatType.CommandSuccess);
    }

    /// <summary>
    ///     Allows a mod to include Singleton, or Transient services to the IOC Container.
    /// </summary>
    /// <param name="services">The service collection.</param>
    public void ConfigureClientModServices(IServiceCollection services, ICoreClientAPI capi)
    {
        services.AddTransient(sp => sp.Resolve<WorldMapManager>().WaypointMapLayer());

        services.AddSingleton<WaypointTemplateService>();
        services.AddSingleton<WaypointCommandsRepository>();
        services.AddSingleton<WaypointQueriesRepository>();

        services.AddTransient<WaypointImportDialogue>();
        services.AddTransient<WaypointExportDialogue>();
        services.AddTransient(sp => new ShowConfirmExportDialogue(p => sp.CreateInstance<WaypointExportConfirmationDialogue>(p)));
    }

    public override void StartClientSide(ICoreClientAPI capi)
    {
        capi.Network.RegisterChannel(nameof(WaypointManager))
            .RegisterMessageType<WorldMapTeleportPacket>()
            .RegisterMessageType<WaypointActionPacket>();

        capi.AddModMenuDialogue<WaypointExportDialogue>(nameof(WaypointManager));
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(WaypointMapLayer), MethodType.Constructor, [typeof(ICoreAPI), typeof(IWorldMapManager)])]
    public static void Harmony_WaypointMapLayer_Constructor_Postfix(WaypointMapLayer __instance)
    {
        var colours = NamedColour
            .ValuesList()
            .Select(x => x.ToColour())
            .FilterSimilarColours(threshold: 20)
            .ToList();

        colours.Sort(new ColourRampComparer { Repetitions = 18, SmoothHueBlending = true });

        __instance.WaypointColors = [.. colours.Select(x => x.ToArgb())];
    }

    /// <summary>
    ///     Prefix patch for <see cref="ClientEventManager.TriggerNewServerChatLine"/> to control whether the server chat line is processed based on the chat type and message content.
    /// </summary>
    /// <param name="message">The chat message being processed.</param>
    /// <param name="chattype">The type of chat message being processed.</param>
    /// <returns>Returns false to prevent further processing of the message if certain conditions are met; otherwise, true.</returns>
    [HarmonyPrefix]
    [HarmonyPatch(typeof(ClientEventManager), nameof(ClientEventManager.TriggerNewServerChatLine))]
    public static bool Harmony_ClientEventManager_TriggerNewServerChatLine_Prefix(string message, EnumChatType chattype)
    {
        if (chattype != EnumChatType.CommandSuccess)
        {
            return true;
        }
        if (message.ContainsNoneOf(["Ok, waypoint", "Ok, deleted waypoint"]))
        {
            return true;
        }

        var retVal = ShowFeedbackMessages;
        return retVal;
    }

    /// <summary>
    ///     Controls whether the output of server chat messages should be silenced under specific conditions.
    /// </summary>
    /// <remarks>
    ///     If <c>true</c>, chat output will be suppressed for certain messages.
    ///     If <c>false</c>, chat output will be processed normally.
    /// </remarks>
    internal static bool ShowFeedbackMessages { get; set; } = true;
}