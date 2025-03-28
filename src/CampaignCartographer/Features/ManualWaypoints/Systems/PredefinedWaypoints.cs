using ApacheTech.VintageMods.CampaignCartographer.Features.ManualWaypoints.Commands;
using ApacheTech.VintageMods.CampaignCartographer.Features.ManualWaypoints.Dialogue;
using ApacheTech.VintageMods.CampaignCartographer.Features.ManualWaypoints.Dialogue.WaypointTemplatePacks;
using ApacheTech.VintageMods.CampaignCartographer.Features.ManualWaypoints.Model;
using ApacheTech.VintageMods.CampaignCartographer.Features.ModMenu.Extensions;
using Gantry.Core.Hosting.Registration;
using Gantry.Services.FileSystem.Abstractions.Contracts;
using Gantry.Services.FileSystem.Enums;
using Gantry.Services.FileSystem.Hosting;

namespace ApacheTech.VintageMods.CampaignCartographer.Features.ManualWaypoints.Systems;

/// <summary>
///     Feature: Manual Waypoint Addition
///      • Contains a GUI that can be used to control the settings for the feature.
///      • Add a waypoint at the player's current location, via a chat command.
/// </summary>
/// <seealso cref="ClientModSystem" />  
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public sealed class PredefinedWaypoints : ClientModSystem, IClientServiceRegistrar
{
    private PredefinedWaypointsSettings _settings;
    private IFileSystemService _fileSystemService;

    public static Dictionary<FileScope, TemplatePack> CustomPacks { get; } = [];
    public static List<TemplatePack> TemplatePacks { get; } = [];

    public void ConfigureClientModServices(IServiceCollection services, ICoreClientAPI capi)
    {
        services.AddFeatureWorldSettings<PredefinedWaypointsSettings>();
        services.TryAddSingleton<PredefinedWaypointsChatCommand>();
        services.TryAddTransient<WaypointTemplatePackDialogue>();
        services.TryAddTransient<EditBlockSelectionWaypointDialogue>();
    }

    protected override void StartPreClientSide(ICoreClientAPI capi)
    {
        G.Log("Starting the manual waypoint service.");
        AssetCategory.categories["templates"] = new AssetCategory("templates", false, EnumAppSide.Client);
        _settings = IOC.Services.GetRequiredService<PredefinedWaypointsSettings>();
        _fileSystemService = IOC.Services.GetRequiredService<IFileSystemService>()
            .RegisterFile("trader-colours.json", FileScope.Global)
            .RegisterFile("custom-global-template-pack.json", FileScope.Global)
            .RegisterFile("custom-world-template-pack.json", FileScope.World);
    }

    private void RegisterTemplatePack(string fileName)
    {
        var templatePack = _fileSystemService.GetJsonFile(fileName).ParseAs<TemplatePack>();
        if (!TemplatePacks.AddIfNotPresent(templatePack)) return;
        CustomPacks[templatePack.Metadata.Scope] = templatePack;
        G.Log($" - {templatePack.Metadata.Title} ({templatePack.Templates.Count} templates)");
    }

    public override void AssetsLoaded(ICoreClientAPI api)
    {
        TemplatePacks.Clear();
        G.Log("Loading custom template packs.");
        RegisterTemplatePack("custom-world-template-pack.json");
        RegisterTemplatePack("custom-global-template-pack.json");

        G.Log("Loading template packs from assets.");
        var templatePacks = Capi.Assets.GetMany<TemplatePack>(G.Logger, pathBegins: "templates", domain: ModEx.ModInfo.ModID);
        foreach (var templatePack in templatePacks.Values)
        {
            if (!TemplatePacks.AddIfNotPresent(templatePack)) continue;
            templatePack.Metadata.Enabled = !_settings.DisabledTemplatePacks.Contains(templatePack.Metadata.Name);
            G.Log($" - {templatePack.Metadata.Title} v{templatePack.Metadata.Version} ({templatePack.Templates.Count} templates)");
        }
    }

    public override void StartClientSide(ICoreClientAPI capi)
    {
        capi.AddModMenuDialogue<WaypointTemplatePackDialogue>("PredefinedWaypoints");
        capi.AddModMenuDialogue<EditBlockSelectionWaypointDialogue>("BlockSelection");

        var oldCommand = IOC.Services.GetRequiredService<PredefinedWaypointsChatCommand>();
        var newCommand = capi.ChatCommands.Create("wp")
            .WithDescription(oldCommand.GetDescription())
            .WithAdditionalInformation(oldCommand.GetSyntax())
            .IgnoreAdditionalArgs()
            .HandleWith(args =>
            {
                oldCommand.CallHandler(args.Caller.Player, args.Caller.FromChatGroupId, args.RawArgs);
                return TextCommandResult.Deferred;
            });
    }
}