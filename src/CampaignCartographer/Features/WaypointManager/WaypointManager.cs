using ApacheTech.VintageMods.CampaignCartographer.Features.WaypointManager.Dialogues.Exports;
using ApacheTech.VintageMods.CampaignCartographer.Features.WaypointManager.Dialogues.Imports;
using ApacheTech.VintageMods.CampaignCartographer.Features.ModMenu.Extensions;
using Gantry.Core.Hosting.Registration;
using Gantry.Core.GameContent.AssetEnum;
using ApacheTech.VintageMods.CampaignCartographer.Features.WaypointManager.Repositories;
using ApacheTech.VintageMods.CampaignCartographer.Features.WaypointManager.WaypointTemplates;
using Gantry.Core.Maths;
using Vintagestory.API.Server;
using Gantry.Services.Network;
using Gantry.Core.Annotation;
using Gantry.Services.Network.Extensions;
using Gantry.Services.Network.Packets;

namespace ApacheTech.VintageMods.CampaignCartographer.Features.WaypointManager;

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
[HarmonyClientSidePatch]
public sealed class WaypointManager : UniversalModSystem, IClientServiceRegistrar
{
    public override double ExecuteOrder() => 0.11;

    public override void StartServerSide(ICoreServerAPI api)
    {
        IOC.Services
            .GetRequiredService<IServerNetworkService>()
            .DefaultServerChannel
            .RegisterMessageHandler<WorldMapTeleportPacket>(OnTeleportPacketReceived);
    }

    [ServerSide]
    private void OnTeleportPacketReceived(IServerPlayer fromPlayer, WorldMapTeleportPacket packet)
    {
        fromPlayer.Entity.TeleportTo(packet.Position);
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
        IOC.Services
            .GetRequiredService<IClientNetworkService>()
            .DefaultClientChannel
            .RegisterMessageType<WorldMapTeleportPacket>();

        capi.AddModMenuDialogue<WaypointExportDialogue>("WaypointManager");
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