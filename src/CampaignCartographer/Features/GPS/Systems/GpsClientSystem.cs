namespace CampaignCartographer.Features.GPS.Systems;

/// <summary>
///     [GPS] The user should be able to display their current XYZ position.
///     [GPS] The user should be able to send their current XYZ position via chat, to other server members.
///     [GPS] The user should be able to copy their current XYZ position to the clipboard.
///     [GPS] The user should be able to send their current XYZ position to a specified player.
///     [GPS] The user should be able to send their current XYZ position to other players, as a clickable link that sets a waypoint on their map.
/// </summary>
/// <seealso cref="ClientModSystem" />
/// <seealso cref="IClientServiceRegistrar" />
internal class GpsClientSystem : ClientModSystem, IClientServiceRegistrar
{
    public void ConfigureClientModServices(IServiceCollection services, ICoreClientAPI capi)
    {
    }
}