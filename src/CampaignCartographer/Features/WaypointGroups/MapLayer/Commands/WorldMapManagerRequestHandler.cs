using ApacheTech.Common.BrighterSlim;
using Gantry.Services.Brighter.Abstractions;

namespace ApacheTech.VintageMods.CampaignCartographer.Features.WaypointGroups.MapLayer.Commands;

/// <summary>
///     Handles requests related to the world map manager.
/// </summary>
/// <typeparam name="T">The type of command being handled.</typeparam>
/// <remarks>
///     This class ensures that the world map dialogue is properly managed when handling commands.
/// </remarks>
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
    public override T Handle(T command)
    {
        MapManager.worldMapDlg = null;
        MapManager.ToggleMap(EnumDialogType.HUD);
        return base.Handle(command);
    }
}