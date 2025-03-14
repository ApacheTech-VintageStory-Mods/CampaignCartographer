using ApacheTech.VintageMods.CampaignCartographer.Features.GPS.Extensions;
using Gantry.Services.Network;
using Gantry.Services.Network.Extensions;

namespace ApacheTech.VintageMods.CampaignCartographer.Features.GPS.Systems;

/// <summary>
///     [GPS] The user should be able to display their current XYZ position.
///     [GPS] The user should be able to send their current XYZ position via chat, to other server members.
///     [GPS] The user should be able to copy their current XYZ position to the clipboard.
///     [GPS] The user should be able to send their current XYZ position to a specified player.
///     [GPS] The user should be able to send their current XYZ position to other players, as a clickable link that sets a waypoint on their map.
/// </summary>
/// <seealso cref="ClientModSystem" />
[UsedImplicitly]
internal class GpsClientSystem : ClientModSystem
{
    /// <inheritdoc />
    public override void StartClientSide(ICoreClientAPI capi)
    {
        G.Logger.VerboseDebug("Starting GPS service.");
        IOC.Services
            .GetRequiredService<IClientNetworkService>()
            .GetOrRegisterChannel("CC_GPS")
            .RegisterMessageHandler<GpsPacket>(p =>
            {
                var message = capi.World.Player.GpsLocation();
                switch (p.Action)
                {
                    case GpsAction.Broadcast:
                        Capi.SendChatMessage(message);
                        break;
                    case GpsAction.Clipboard:
                        Capi.Input.ClipboardText = message;
                        break;
                    case GpsAction.Notification:
                        Capi.ShowChatMessage(message);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(p));
                }
            });
    }
}