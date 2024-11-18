using ApacheTech.VintageMods.CampaignCartographer.Features.ModMenu.Extensions;
using ApacheTech.VintageMods.CampaignCartographer.Features.Waypoints.Automatic.Dialogue;
using Gantry.Core.Hosting.Registration;
using Gantry.Services.FileSystem.Abstractions.Contracts;
using Gantry.Services.FileSystem.Enums;
using Gantry.Services.FileSystem.Hosting;

namespace ApacheTech.VintageMods.CampaignCartographer.Features.Waypoints.Automatic;

/// <summary>
///     Automatic Waypoint Addition (.wpAuto)
///      - Contains a GUI that can be used to control the settings for the feature (Shift + F7).
///      - Enable / Disable all automatic waypoint placements.
///      - Automatically add waypoints for Translocators, as the player travels between them.
///      - Automatically add waypoints for Teleporters, as the player travels between them.
///      - Automatically add waypoints for Traders, as the player interacts with them.
///      - Automatically add waypoints for Meteors, when the player punches a Meteoric Iron Block.
///      - Server: Send Teleporter information to clients, when creating Teleporter waypoints.
/// </summary>
/// <seealso cref="ClientModSystem" />
[UsedImplicitly]
public sealed class AutoWaypoints : ClientModSystem, IClientServiceRegistrar
{
    /// <summary>
    ///     Allows a mod to include Singleton, or Transient services to the IOC Container.
    /// </summary>
    /// <param name="services">The service collection.</param>
    public void ConfigureClientModServices(IServiceCollection services, ICoreClientAPI capi)
    {
        services.AddFeatureWorldSettings<AutoWaypointsSettings>();
        services.AddSingleton<AutoWaypointsDialogue>();
        services.AddSingleton<AutoWaypointPatchHandler>();
    }

    /// <summary>
    ///     Called during initial mod loading, called before any mod receives the call to Start().
    /// </summary>
    /// <param name="capi"></param>
    protected override void StartPreClientSide(ICoreClientAPI capi)
    {
        IOC.Services
            .Resolve<IFileSystemService>()
            .RegisterFile("crossmap.json", FileScope.Global);
    }

    /// <summary>
    ///     Minor convenience method to save yourself the check for/cast to ICoreClientAPI in Start()
    /// </summary>
    /// <param name="capi">The client-side API.</param>
    public override void StartClientSide(ICoreClientAPI capi)
    {
        capi.Event.LevelFinalize += () =>
        {
            capi.ChatCommands.Create("wpAuto")
                .WithDescription(LangEx.FeatureString("AutoWaypoints", "SettingsCommandDescription"))
                .HandleWith(args =>
                {
                    IOC.Services.Resolve<AutoWaypointsDialogue>().ToggleGui();
                    return TextCommandResult.Success();
                });

            capi.AddModMenuDialogue<AutoWaypointsDialogue>("AutoWaypoints");
        };
    }
}