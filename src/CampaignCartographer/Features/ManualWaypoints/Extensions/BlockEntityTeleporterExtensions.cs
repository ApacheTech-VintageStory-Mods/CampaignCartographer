using ApacheTech.Common.Extensions.Harmony;
using ApacheTech.VintageMods.CampaignCartographer.Features.WaypointManager.Extensions;
using ApacheTech.VintageMods.CampaignCartographer.Features.WaypointManager.WaypointTemplates;
using Gantry.Core.GameContent.AssetEnum;

namespace ApacheTech.VintageMods.CampaignCartographer.Features.ManualWaypoints.Extensions;

/// <summary>
///     Provides extension methods for <see cref="BlockEntityTeleporterBase"/> and related functionality.
/// </summary>
public static class BlockEntityTeleporterExtensions
{
    private static WaypointTemplateService _waypointService;

    /// <summary>
    ///     Adds waypoints at each end of a teleporter.
    /// </summary>
    /// <param name="teleporter">The teleporter to add the waypoints to.</param>
    /// <param name="titleTemplate">The code within the lang file, for the title template of the waypoint.</param>
    /// <remarks>
    ///     This method ensures that a waypoint is added at the teleporter's source position
    ///     if one does not already exist. The waypoint title is generated using the provided lang file
    ///     code, and the teleporter's source and target names.
    /// </remarks>
    public static void AddWaypoint(this BlockEntityTeleporterBase teleporter, string titleTemplate)
    {
        _waypointService ??= IOC.Services.Resolve<WaypointTemplateService>();
        var tpLocation = teleporter.GetField<TeleporterLocation>("tpLocation");
        var sourcePos = teleporter.Pos;

        if (sourcePos.WaypointExistsAtPos(p => p.Icon == WaypointIcon.Spiral)) return;

        var sourceName = tpLocation.SourceName?.IfNullOrWhitespace("Unknown");
        var targetName = tpLocation.TargetName?.IfNullOrWhitespace("Unknown");

        var title = Lang.Get(titleTemplate, sourceName, targetName);

        new PredefinedWaypointTemplate
        {
            Title = title,
            Colour = NamedColour.SpringGreen,
            DisplayedIcon = WaypointIcon.Spiral,
            ServerIcon = WaypointIcon.Spiral
        }.AddToMap(sourcePos);

        G.Logger.VerboseDebug($"Added Waypoint: {title}");
    }
}