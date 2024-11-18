using System.IO;
using System.Text;
using ApacheTech.VintageMods.CampaignCartographer.Features.ModMenu.Extensions;
using ApacheTech.VintageMods.CampaignCartographer.Features.WaypointManager.WaypointTemplates;
using ApacheTech.VintageMods.CampaignCartographer.Features.Waypoints.Manual.Commands;
using ApacheTech.VintageMods.CampaignCartographer.Features.Waypoints.Manual.Dialogue;
using ApacheTech.VintageMods.CampaignCartographer.Features.Waypoints.Manual.Dialogue.PredefinedWaypoints;
using Gantry.Core.Hosting.Registration;
using Gantry.Services.FileSystem;
using Gantry.Services.FileSystem.Abstractions.Contracts;
using Gantry.Services.FileSystem.Enums;
using Gantry.Services.FileSystem.Extensions;
using Gantry.Services.FileSystem.Hosting;

namespace ApacheTech.VintageMods.CampaignCartographer.Features.Waypoints.Manual.Systems;

/// <summary>
///     Feature: Manual Waypoint Addition
///      • Contains a GUI that can be used to control the settings for the feature.
///      • Add a waypoint at the player's current location, via a chat command.
/// </summary>
/// <seealso cref="ClientModSystem" />  
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public sealed class PredefinedWaypoints : ClientModSystem, IClientServiceRegistrar
{
    /// <summary>
    ///     Allows a mod to include Singleton, or Transient services to the IOC Container.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="capi">The client-side API.</param>
    public void ConfigureClientModServices(IServiceCollection services, ICoreClientAPI capi)
    {
        services.AddFeatureWorldSettings<PredefinedWaypointsSettings>();
        services.TryAddSingleton<PredefinedWaypointsChatCommand>();
        services.TryAddTransient<PredefinedWaypointsDialogue>();
        services.TryAddTransient<EditBlockSelectionWaypointDialogue>();
    }

    /// <summary>
    ///     Called on the client, during initial mod loading, called before any mod receives the call to Start().
    /// </summary>
    /// <param name="capi">
    ///     The core API implemented by the client.
    ///     The main interface for accessing the client.
    ///     Contains all sub-components, and some miscellaneous methods.
    /// </param>
    protected override void StartPreClientSide(ICoreClientAPI capi)
    {
        IOC.Services.Resolve<IFileSystemService>()
            .RegisterFile("trader-colours.json", FileScope.Global);
    }

    public override void StartClientSide(ICoreClientAPI capi)
    {
        capi.AddModMenuDialogue<PredefinedWaypointsDialogue>("PredefinedWaypoints");
        capi.AddModMenuDialogue<EditBlockSelectionWaypointDialogue>("BlockSelection");

        var oldCommand = IOC.Services.Resolve<PredefinedWaypointsChatCommand>();
        var newCommand = capi.ChatCommands.Create("wp")
            .WithDescription(oldCommand.GetDescription())
            .WithAdditionalInformation(oldCommand.GetSyntax())
            .IgnoreAdditionalArgs()
            .HandleWith(args =>
            {
                oldCommand.CallHandler(args.Caller.Player, args.Caller.FromChatGroupId, args.RawArgs);
                return TextCommandResult.Deferred;
            });


        UpdateWaypointTypesFromWorldFile();
    }

    private static void UpdateWaypointTypesFromWorldFile()
    {
        var oldFile = new FileInfo(Path.Combine(ModPaths.ModDataWorldPath, "waypoint-types.json"));
        if (!oldFile.Exists) return;

        var newFile = IOC.Services.Resolve<IFileSystemService>().GetJsonFile("waypoint-types.json");
        var waypointTypes = new SortedDictionary<string, PredefinedWaypointTemplate>();

        waypointTypes.AddOrUpdateRange(newFile.ParseAsMany<PredefinedWaypointTemplate>(), w => w.Key);
        waypointTypes.AddOrUpdateRange(oldFile.ParseAsMany<PredefinedWaypointTemplate>(), w => w.Key);

        newFile.SaveFromList(waypointTypes.Values);
        oldFile.Delete();
    }
}