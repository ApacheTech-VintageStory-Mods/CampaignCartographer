using ApacheTech.VintageMods.CampaignCartographer.Features.ModMenu.Dialogue;
using Gantry.Core.Hosting.Registration;

// ReSharper disable StringLiteralTypo

namespace ApacheTech.VintageMods.CampaignCartographer.Features.ModMenu;

/// <summary>
///     Provides a main GUI for the mod, as a central location to access each feature; rather than through commands.
/// </summary>
/// <seealso cref="ClientModSystem" />
[UsedImplicitly]
public sealed class ModMenuClientSystem : ClientModSystem, IClientServiceRegistrar
{
    public override double ExecuteOrder() => 0;

    public void ConfigureClientModServices(IServiceCollection services, ICoreClientAPI capi)
    {
        services.AddTransient<ModMenuDialogue>();
        services.AddTransient<SupportDialogue>();
    }

    public override void StartClientSide(ICoreClientAPI capi)
    {
        capi.Input.RegisterTransientGuiDialogueHotKey(
            IOC.Services.Resolve<ModMenuDialogue>, LangEx.ModTitle(), GlKeys.F7);

        capi.ChatCommands
            .Create("wpsettings")
            .WithDescription(LangEx.FeatureString("ManualWaypoints", "SettingsCommandDescription"))
            .HandleWith(_ =>
            {
                IOC.Services.Resolve<ModMenuDialogue>().ToggleGui();
                return TextCommandResult.Success();
            });
    }

    public Dictionary<Type, string> FeatureDialogues { get; } = [];

    public override void Dispose() => FeatureDialogues.Clear();
}