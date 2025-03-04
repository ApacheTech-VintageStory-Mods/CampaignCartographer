namespace ApacheTech.VintageMods.CampaignCartographer.Features.WaypointGroups.Abstractions;

public interface IMapLayerGeneration
{
    /// <summary>
    ///     Pauses the generation thread.
    /// </summary>
    void Suspend();

    /// <summary>
    ///     Resumes the generation thread.
    /// </summary>
    void Resume();
}
