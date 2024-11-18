using ApacheTech.VintageMods.CampaignCartographer.Features.WaypointManager.Dialogues.Exports;
using ApacheTech.VintageMods.CampaignCartographer.Features.WaypointManager.Dialogues.Imports;
using ApacheTech.VintageMods.CampaignCartographer.Features.ModMenu.Extensions;
using Gantry.Core.Hosting.Registration;
using Gantry.Core.GameContent.AssetEnum;
using ApacheTech.VintageMods.CampaignCartographer.Features.WaypointManager.Repositories;
using ApacheTech.VintageMods.CampaignCartographer.Features.WaypointManager.WaypointTemplates;

namespace ApacheTech.VintageMods.CampaignCartographer.Features.WaypointManager;

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
[HarmonySidedPatch(EnumAppSide.Client)]
public sealed class WaypointManager : ClientModSystem, IClientServiceRegistrar
{
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
        capi.AddModMenuDialogue<WaypointExportDialogue>("WaypointManager");
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(WaypointMapLayer), MethodType.Constructor, [typeof(ICoreAPI), typeof(IWorldMapManager)])]
    public static void Harmony_WaypointMapLayer_Constructor_Postfix(WaypointMapLayer __instance)
    {
        __instance.WaypointColors = NamedColour
            .ValuesList()
            .Select(x => x.ToColour().ToArgb())
            .ToList();
    }
}