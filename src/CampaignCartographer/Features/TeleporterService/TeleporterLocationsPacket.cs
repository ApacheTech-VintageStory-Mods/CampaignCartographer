namespace ApacheTech.VintageMods.CampaignCartographer.Features.TeleporterService;

/// <summary>
///     Represents a network packet containing a list of teleporter locations.
/// </summary>
[ProtoContract]
public class TeleporterLocationsPacket
{
    /// <summary>
    ///     The list of teleporter locations included in the packet.
    /// </summary>
    [ProtoMember(1)]
    [UsedImplicitly]
    public List<TeleporterLocation> Teleporters { get; set; } = [];
}