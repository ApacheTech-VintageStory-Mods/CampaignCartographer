using ApacheTech.VintageMods.CampaignCartographer.Features.AutoWaypoints.Dialogue;
using ApacheTech.VintageMods.CampaignCartographer.Features.ModMenu.Extensions;
using ApacheTech.VintageMods.CampaignCartographer.Features.WaypointManager.Repositories.Commands;
using Gantry.Core.Hosting.Registration;
using Gantry.Services.FileSystem.Abstractions.Contracts;
using Gantry.Services.FileSystem.Enums;
using Gantry.Services.FileSystem.Hosting;

namespace ApacheTech.VintageMods.CampaignCartographer.Features.AutoWaypoints;

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
    private AutoWaypointsSettings _settings;

    /// <summary>
    ///     Allows a mod to include Singleton, or Transient services to the IOC Container.
    /// </summary>
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
        G.Log("Starting automatic waypoint service.");
        _settings = IOC.Services.GetRequiredService<AutoWaypointsSettings>();
        capi.Event.LevelFinalize += () =>
        {
            _settings.AddPropertyChangedAction(p => p.DeathWaypoints, enabled =>
            {
                capi.ShowChatMessage($"Automatic death waypoints have been turned {State(!enabled)}. Turning vanilla death waypoints {State(enabled)}.");
                capi.SendChatMessage($"/waypoint deathWp {State(enabled)}");
                return;
                string State(bool state) => state ? "off" : "on";
            });

            capi.AddModMenuDialogue<AutoWaypointsDialogue>("AutoWaypoints");
            var player = capi.World.Player.Entity;
            player.WatchedAttributes.RegisterModifiedListener("entityDead", () =>
            {
                if (!_settings.DeathWaypoints) return;
                if (player.WatchedAttributes?["entityDead"] is null) return;
                if (player.WatchedAttributes.GetInt("entityDead") != 1) return;
               
                var title = LangEx.FeatureString("AutoWaypoints", "DeathWaypointTitle", DateTime.Now.ToString("MMM dd, yyyy HH:mm"));
                var command = new AddWaypointCommand(
                    position: player.Pos.AsBlockPos, 
                    icon: "gravestone", 
                    pinned: true, 
                    colour: ColourEx.RandomClampedHexColour(capi, 0.5, 0.8),
                    title: title);
                command.Execute();
                
                G.Log($"Added death marker: {title}");
            });
        };
    }
}