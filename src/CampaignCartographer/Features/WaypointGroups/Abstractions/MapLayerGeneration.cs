using System.Threading;

namespace ApacheTech.VintageMods.CampaignCartographer.Features.WaypointGroups.Abstractions;

public class MapLayerGeneration : IMapLayerGeneration
{
    /// <summary>
    ///     A static lock object used to synchronise access to the MapLayers collection.
    /// </summary>
    public static object MapLayersLock { get; } = new();

    /// <summary>
    ///     A static ManualResetEventSlim used to pause and resume the generation thread.
    ///     When set (true), the thread runs; when reset (false), the thread waits.
    /// </summary>
    public static ManualResetEventSlim ThreadControl { get; } = new(true);

    /// <summary>
    ///     Pauses the generation thread.
    /// </summary>
    public void Suspend() => ThreadControl.Reset();

    /// <summary>
    ///     Resumes the generation thread.
    /// </summary>
    public void Resume() => ThreadControl.Set();
}
