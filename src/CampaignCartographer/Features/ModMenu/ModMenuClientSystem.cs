using ApacheTech.VintageMods.CampaignCartographer.Features.ModMenu.Dialogue;
using Gantry.Core.Hosting.Registration;

// ReSharper disable StringLiteralTypo

namespace ApacheTech.VintageMods.CampaignCartographer.Features.ModMenu;

/// <summary>
///     Represents the main GUI for the mod, providing a central location to access all features rather than relying on commands.
/// </summary>
/// <seealso cref="ClientModSystem" />
[UsedImplicitly]
public sealed class ModMenuClientSystem : ClientModSystem, IClientServiceRegistrar
{
    /// <summary>
    ///     Specifies the execution order of the system.
    /// </summary>
    /// <returns>A double value representing the execution order.</returns>
    public override double ExecuteOrder() => -1;

    /// <summary>
    ///     Configures the client-side services for the mod.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <param name="capi">The core client API.</param>
    public void ConfigureClientModServices(IServiceCollection services, ICoreClientAPI capi)
    {
        services.AddTransient<ModMenuDialogue>();
        services.AddTransient<SupportDialogue>();
    }

    /// <summary>
    ///     Starts the client-side functionality of the mod.
    /// </summary>
    /// <param name="capi">The core client API.</param>
    public override void StartClientSide(ICoreClientAPI capi)
    {
        ApiEx.Logger.VerboseDebug("Starting mod menu service");

        // Registers a hotkey to open the mod menu dialogue
        capi.Input.RegisterTransientGuiDialogueHotKey(
            IOC.Services.Resolve<ModMenuDialogue>,
            LangEx.ModTitle(),
            GlKeys.F7);
    }

    /// <summary>
    ///     Gets a dictionary mapping feature types to their associated dialogue names.
    /// </summary>
    public Dictionary<Type, string> FeatureDialogues { get; } = new();

    /// <summary>
    ///     Cleans up the resources used by the system.
    /// </summary>
    public override void Dispose() => FeatureDialogues.Clear();
}