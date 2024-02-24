using ApacheTech.VintageMods.CampaignCartographer.Domain.ChatCommands.Extensions;
using Gantry.Core.Extensions.GameContent;
using Gantry.Services.HarmonyPatches.DependencyInjection;
using Gantry.Services.Network.DependencyInjection;

namespace ApacheTech.VintageMods.CampaignCartographer;

/// <summary>
///     Entry-point for the mod. This class will configure and build the IOC Container, and Service list for the rest of the mod.
///     
///     Registrations performed within this class should be global scope; by convention, features should aim to be as stand-alone as they can be.
/// </summary>
/// <remarks>
///     Only one derived instance of this class should be added to any single mod within
///     the VintageMods domain. This class will enable Dependency Injection, and add all
///     of the domain services. Derived instances should only have minimal functionality, 
///     instantiating, and adding Application specific services to the IOC Container.
/// </remarks>
/// <seealso cref="ModHost" />
[UsedImplicitly]
public sealed class Program : ModHost
{
    /// <summary>
    /// Configures any services that need to be added to the IO Container, on the client side.
    /// </summary>
    /// <param name="services">The as-of-yet un-built services container.</param>
    /// <param name="api">Access to the universal API.</param>
    protected override void ConfigureUniversalModServices(IServiceCollection services, ICoreAPI api)
    {
        services.AddFileSystemService(o => o.RegisterSettingsFiles = true);
        services.AddHarmonyPatchingService(o => o.AutoPatchModAssembly = true);
        services.AddNetworkService();

        // TODO: ROADMAP: This may be better to push to a separate mod system.
        services.AddProprietaryModSystem<IWorldMapManager, WorldMapManager>();
        services.AddSingleton(sp => sp.Resolve<WorldMapManager>().WaypointMapLayer());
    }
}
