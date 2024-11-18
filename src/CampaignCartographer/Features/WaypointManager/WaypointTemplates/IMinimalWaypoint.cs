namespace ApacheTech.VintageMods.CampaignCartographer.Features.WaypointManager.WaypointTemplates;

public interface IMinimalWaypoint
{
    public string Guid { get; set; }

    public string Title { get; set; }

    public int Color { get; set; }

    public string Icon { get; set; }

    public bool Pinned { get; set; }
}