using ApacheTech.VintageMods.CampaignCartographer.Features.WaypointManager.WaypointTemplates;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.MathTools;

namespace ApacheTech.VintageMods.CampaignCartographer.Features.WaypointManager.Models;

/// <summary>
///     Represents a JSON file that contains exported waypoints from a world.
/// </summary>
[JsonObject]
public class WaypointFileModel
{
    /// <summary>
    ///     Gets or sets the name given to this export file.
    /// </summary>
    /// <value>The name that is displayed in the imports screen.</value>
    public string Name { get; set; } = default!;

    /// <summary>
    ///     Gets or sets the world from which these waypoints were exported.
    /// </summary>
    /// <value>The world from which these waypoints were exported.</value>
    public string World { get; set; } = default!;

    /// <summary>
    ///     Gets or sets the number of waypoints contained within the file.
    /// </summary>
    /// <value>The number of waypoints contained within the file.</value>
    public int Count { get; set; }

    /// <summary>
    ///     Gets or sets the date and time the export file was created.
    /// </summary>
    /// <value>The creation date of the export file.</value>
    public DateTime DateCreated { get; set; }

    /// <summary>
    ///     Gets or sets the spawn position.
    /// </summary>
    /// <value>The spawn position.</value>
    public EntityPos SpawnPosition { get; set; } = default!;

    /// <summary>
    ///     Gets or sets a list of waypoints contained within the export file.
    /// </summary>
    /// <value>The list of exported waypoints.</value>
    public List<PositionedWaypointTemplate> Waypoints { get; set; } = default!;

    /// <summary>
    ///     
    /// </summary>
    public Vec3d Relative(PositionedWaypointTemplate waypoint)
    {
        var pos = SpawnPosition.XYZ + waypoint.Position;
        return pos;
    }
}