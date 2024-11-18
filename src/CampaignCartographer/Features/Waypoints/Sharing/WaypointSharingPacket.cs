using ProtoBuf;

namespace ApacheTech.VintageMods.CampaignCartographer.Features.Waypoints.Sharing;

[ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
public class WaypointSharingPacket
{
    public string Message { get; set; } = string.Empty;

    public static WaypointSharingPacket WithMessage(string message)
        => new() { Message = message };
}
