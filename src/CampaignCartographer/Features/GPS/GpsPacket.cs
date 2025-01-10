namespace ApacheTech.VintageMods.CampaignCartographer.Features.GPS;

/// <summary>
///     A packet sent between the client and server, detailing information about a whisper sent between two online players.
/// </summary>
[ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public class GpsPacket
{
    public GpsAction Action { get; set; }
}