using ApacheTech.VintageMods.CampaignCartographer.Features.ModMenu.Extensions;
using ApacheTech.VintageMods.CampaignCartographer.Features.PlayerPins.ChatCommands;
using ApacheTech.VintageMods.CampaignCartographer.Features.PlayerPins.Dialogue;
using Gantry.Core.GameContent.ChatCommands;
using Gantry.Core.Hosting.Registration;
using Gantry.Services.FileSystem.Hosting;

// ReSharper disable StringLiteralTypo

namespace ApacheTech.VintageMods.CampaignCartographer.Features.PlayerPins;

/// <summary>
///     Client-side entry point for the PlayerPins feature.
/// </summary>
/// <seealso cref="ClientModSystem" />
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public class PlayerPins : ClientModSystem, IClientServiceRegistrar
{
    /// <summary>
    ///     Allows a mod to include Singleton, or Transient services to the IOC Container.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="capi">The client side API.</param>
    public void ConfigureClientModServices(IServiceCollection services, ICoreClientAPI capi)
    {
        services.AddFeatureWorldSettings<PlayerPinsSettings>();
        services.AddSingleton<PlayerPinsDialogue>();
        services.AddSingleton<HighlightClientChatCommand>();
    }

    /// <inheritdoc />
    public override void StartClientSide(ICoreClientAPI capi)
    {
        G.Log.VerboseDebug("Starting player pins system");
        capi.ChatCommands
            .Create("playerpins")
            .WithDescription(LangEx.FeatureString("PlayerPins", "SettingsCommandDescription"))
            .HandleWith(_ => IOC.Services.Resolve<PlayerPinsDialogue>().ToggleGui());

        IOC.Services.Resolve<HighlightClientChatCommand>().Register();

        capi.AddModMenuDialogue<PlayerPinsDialogue>("PlayerPins");
    }
}