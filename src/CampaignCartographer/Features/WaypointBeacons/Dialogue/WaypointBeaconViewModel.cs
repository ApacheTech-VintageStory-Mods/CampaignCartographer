using Vintagestory.API.MathTools;
using Vintagestory.API.Util;

namespace ApacheTech.VintageMods.CampaignCartographer.Features.WaypointBeacons.Dialogue;

/// <summary>
///     Represents a view model for a waypoint beacon, providing information about its state, position, and visibility.
/// </summary>
public class WaypointBeaconViewModel
{
    private readonly string _id;
    private readonly WaypointBeaconsSettings _settings;
    private readonly WaypointMapLayer _mapLayer;
    private Waypoint _memento;

    private Waypoint ThisWaypoint
    {
        get
        {
            try
            {
                if (_memento is not null) return _memento;
                var mapLayer = _mapLayer;
                var ownWaypoints = mapLayer?.ownWaypoints?.ToArray();
                _memento = ownWaypoints?.FirstOrDefault(p => p.Guid == _id);
                return _memento;
            }
            catch
            {
                return null;
            }
        }
    }

    /// <summary>
    ///     Initialises a new instance of the <see cref="WaypointBeaconViewModel"/> class with the specified waypoint ID and settings.
    /// </summary>
    /// <param name="id">The unique identifier of the waypoint.</param>
    /// <param name="settings">The settings for waypoint beacons.</param>
    public WaypointBeaconViewModel(string id, WaypointBeaconsSettings settings)
    {
        _id = id;
        _settings = settings;
        _mapLayer = IOC.Services.GetRequiredService<WaypointMapLayer>();
    }

    /// <summary>
    ///     The waypoint associated with this view model.
    /// </summary>
    public Waypoint Waypoint => Get(p => p);

    /// <summary>
    ///     Indicates whether the waypoint is available.
    /// </summary>
    public bool Available => ThisWaypoint is not null;

    /// <summary>
    ///     The index of the waypoint within the waypoint map layer.
    /// </summary>
    public int Index => Get(p => p.GetIndex());

    /// <summary>
    ///     The position of the waypoint in the world, adjusted to align with visual representation.
    /// </summary>
    public Vec3d WaypointPosition => Get(p => p.Position.AsBlockPos.ToVec3d().SubCopy(0, 0.5, 0).Add(0.5));

    /// <summary>
    ///     The distance from the player's current position to the waypoint.
    /// </summary>
    public double DistanceFromPlayer => Get(_ => ApiEx.Client.World.Player.Entity.Pos.DistanceTo(WaypointPosition));

    /// <summary>
    ///     Indicates whether the waypoint is visible based on its distance from the player and configured range.
    /// </summary>
    public bool Visible => DistanceFromPlayer < _settings.IconRange;

    /// <summary>
    ///     The colour of the waypoint normalised to an array of RGBA double values.
    /// </summary>
    public double[] NormalisedColour => Get(p => ColorUtil.ToRGBADoubles(p.Color));

    /// <summary>
    ///     A formatted label for the waypoint, including optional prefix, index, and distance information.
    /// </summary>
    public string Label => Get(p =>
    {
        var title = p.Title is not null
            ? $"{(_settings.ShowWaypointPrefix ? "Waypoint: " : "")}{(_settings.ShowWaypointIndex
                ? $"ID: {Index} | " : "")}{p.Title}" : "";

        var distance = DistanceFromPlayer >= 1000
            ? $"{Math.Round(DistanceFromPlayer / 1000, 3):F3}km"
            : $"{DistanceFromPlayer:F3}m";

        return $"{title.UcFirst()} {distance}";
    });

    /// <summary>
    ///     Safely retrieves a value based on the waypoint, returning the default value if unavailable.
    /// </summary>
    /// <typeparam name="T">The type of the value to retrieve.</typeparam>
    /// <param name="f">A function to retrieve the value from the waypoint.</param>
    /// <returns>The retrieved value, or the default if the waypoint is unavailable.</returns>
    private T Get<T>(System.Func<Waypoint, T> f)
    {
        try
        {
            return Available ? f(ThisWaypoint) : default;
        }
        catch
        {
            return default;
        }
    }
}