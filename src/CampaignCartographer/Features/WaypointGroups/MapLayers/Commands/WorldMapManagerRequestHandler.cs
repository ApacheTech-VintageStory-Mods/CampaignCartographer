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
    ///     Tthe world map manager instance.
    /// </summary>
    protected WorldMapManager MapManager { get; }

    /// <summary>
    ///     The core client API.
    /// </summary>
    protected ICoreClientAPI Capi { get; }

    /// <summary>
    ///     Initialises a new instance of the <see cref="WorldMapManagerRequestHandler{T}"/> class.
    /// </summary>
    protected WorldMapManagerRequestHandler(WorldMapManager mapManager, ICoreClientAPI capi)
    {
        MapManager = mapManager;
        Capi = capi;
    }

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
            Capi.Gui.TriggerDialogClosed(MapManager.worldMapDlg);
            MapManager.worldMapDlg.Dispose();
            MapManager.worldMapDlg = null;
            MapManager.ToggleMap(EnumDialogType.HUD);
            G.Log($"Rebuilt world map after map layer updates.");
        }
        return base.Handle(command);
    }
}