using ApacheTech.VintageMods.CampaignCartographer.Features.ModMenu.Extensions;
using ApacheTech.VintageMods.CampaignCartographer.Features.PlayerPins.ChatCommands;
using ApacheTech.VintageMods.CampaignCartographer.Features.PlayerPins.DataStructures;
using ApacheTech.VintageMods.CampaignCartographer.Features.PlayerPins.Dialogue;
using Cairo;
using Gantry.Core.GameContent.ChatCommands;
using Gantry.Core.Hosting.Registration;
using Gantry.Services.FileSystem.Hosting;
using Color = System.Drawing.Color;

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
        G.Logger.VerboseDebug("Starting player pins system");
        capi.AddModMenuDialogue<PlayerPinsDialogue>("PlayerPins");

        capi.ChatCommands
            .Create("playerpins")
            .WithDescription(LangEx.FeatureString("PlayerPins", "SettingsCommandDescription"))
            .HandleWith(_ => IOC.Services.Resolve<PlayerPinsDialogue>().ToggleGui());

        IOC.Services.Resolve<HighlightClientChatCommand>().Register();
        LoadTextures();
    }

    public override void Dispose()
    {
        TextureCache.PurgeValues();
    }

    public void LoadTextures()
    {
        var settings = IOC.Services.Resolve<PlayerPinsSettings>();
        TextureCache[PlayerRelation.Self] = LoadTexture(settings.SelfColour, settings.SelfScale);
        TextureCache[PlayerRelation.Highlighted] = LoadTexture(settings.HighlightColour, settings.HighlightScale);
        TextureCache[PlayerRelation.Others] = LoadTexture(settings.OthersColour, settings.OthersScale);
    }

    private LoadedTexture LoadTexture(Color colour, int scale)
    {
        scale += 16;

        var rgba = colour.ToNormalisedRgba();
        var outline = new[] { 0d, 0d, 0d, rgba[3] };

        var imageSurface = new ImageSurface(Format.Argb32, scale, scale);
        var context = new Context(imageSurface);

        context.SetSourceRGBA(0.0, 0.0, 0.0, 0.0);
        context.Paint();
        Capi.Gui.Icons.DrawMapPlayer(context, 0, 0, scale, scale, outline, rgba);
        var texture = Capi.Gui.LoadCairoTexture(imageSurface, false);
        var loadedTexture = new LoadedTexture(Capi, texture, scale, scale);

        // Explicitly disposing the context and surface, after each use.
        context.Dispose();
        imageSurface.Dispose();

        return loadedTexture;
    }

    public Dictionary<PlayerRelation, LoadedTexture> TextureCache { get; } = [];
}