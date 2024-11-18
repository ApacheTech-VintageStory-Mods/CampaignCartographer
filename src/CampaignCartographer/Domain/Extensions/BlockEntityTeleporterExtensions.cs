using ApacheTech.Common.Extensions.Harmony;
using Gantry.Core.GameContent.AssetEnum;
using ApacheTech.VintageMods.CampaignCartographer.Features.WaypointManager.WaypointTemplates;
using ApacheTech.VintageMods.CampaignCartographer.Features.WaypointManager.Extensions;

namespace ApacheTech.VintageMods.CampaignCartographer.Domain.Extensions;

public static class BlockEntityTeleporterExtensions
{
    private static WaypointTemplateService _waypointService;

    /// <summary>
    ///     Adds waypoints at each end of a teleporter.
    /// </summary>
    /// <param name="teleporter">The teleporter to add the waypoints to.</param>
    /// <param name="titleTemplate">The code within the lang file, for the title template of the waypoint.</param>
    public static void AddWaypoint(this BlockEntityTeleporterBase teleporter, string titleTemplate)
    {
        _waypointService ??= IOC.Services.Resolve<WaypointTemplateService>();
        var tpLocation = teleporter.GetField<TeleporterLocation>("tpLocation");
        var sourcePos = teleporter.Pos;

        if (sourcePos.WaypointExistsAtPos(p => p.Icon == WaypointIcon.Spiral)) return;

        var sourceName = tpLocation.SourceName?.IfNullOrWhitespace("Unknown");
        var targetName = tpLocation.TargetName?.IfNullOrWhitespace("Unknown");

        var title = Lang.Get(titleTemplate, sourceName, targetName);

        _waypointService.GetTemplateByKey("tl")?
            .With(p =>
            {
                p.Title = title;
                p.Colour = NamedColour.SpringGreen;
            })
            .AddToMap(sourcePos);
            
        ApiEx.Client.Logger.VerboseDebug($"Added Waypoint: {title}");
    }
}