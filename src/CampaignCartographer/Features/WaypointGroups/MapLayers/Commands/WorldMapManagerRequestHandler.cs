using ApacheTech.Common.BrighterSlim;
using Gantry.Core.Annotation;
using Gantry.Services.Brighter.Abstractions;
using Gantry.Services.Brighter.Filters;

namespace ApacheTech.VintageMods.CampaignCartographer.Features.WaypointGroups.MapLayers.Commands;

/// <summary>
///     Handles requests related to the world map manager.
/// </summary>
/// <typeparam name="T">The type of command being handled.</typeparam>
/// <remarks>
///     This class ensures that the world map dialogue is properly managed when handling commands.
/// </remarks>
[ClientSide]
public class WorldMapManagerRequestHandler<T> : RequestHandler<T> where T : CommandBase
{
    /// <summary>
    ///     Gets the world map manager instance.
    /// </summary>
    protected WorldMapManager MapManager { get; }

    /// <summary>
    ///     Initialises a new instance of the <see cref="WorldMapManagerRequestHandler{T}"/> class.
    /// </summary>
    /// <param name="mapManager">The world map manager instance.</param>
    protected WorldMapManagerRequestHandler(WorldMapManager mapManager) => MapManager = mapManager;

    /// <summary>
    ///     Handles the specified command, ensuring the world map dialogue is managed correctly.
    /// </summary>
    /// <param name="command">The command to handle.</param>
    /// <returns>The processed command.</returns>
    [HandledOnClient]
    public override T Handle(T command)
    {
        if (MapManager.worldMapDlg is not null)
        {
            MapManager.worldMapDlg.Dispose();
            MapManager.worldMapDlg = null;
            MapManager.ToggleMap(EnumDialogType.HUD);
            ApiEx.Logger.VerboseDebug($"Rebuilt world map after map layer updates.");
        }
        return base.Handle(command);
    }
}