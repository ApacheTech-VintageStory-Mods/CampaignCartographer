using ApacheTech.VintageMods.CampaignCartographer.Features.ModMenu.Extensions;
using ApacheTech.VintageMods.CampaignCartographer.Features.PlayerPins.ChatCommands;
using ApacheTech.VintageMods.CampaignCartographer.Features.PlayerPins.Dialogue;

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
    /// <param name="capi">The The client side API.</param>
    public void ConfigureClientModServices(IServiceCollection services, ICoreClientAPI capi)
    {
        services.AddFeatureWorldSettings<PlayerPinsSettings>();
        services.AddSingleton<PlayerPinsDialogue>();
        services.AddSingleton<FriendClientChatCommand>();
    }

    public override void StartClientSide(ICoreClientAPI capi)
    {
        capi.ChatCommands
            .Create("playerpins")
            .WithDescription(LangEx.FeatureString("PlayerPins", "SettingsCommandDescription"))
            .HandleWith(_ =>
            {
                IOC.Services.Resolve<PlayerPinsDialogue>().ToggleGui();
                return TextCommandResult.Success();
            });

        IOC.Services.Resolve<FriendClientChatCommand>().Register();

        capi.AddModMenuDialogue<PlayerPinsDialogue>("PlayerPins");
    }
}