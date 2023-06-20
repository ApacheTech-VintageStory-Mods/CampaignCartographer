namespace CampaignCartographer.Features.GPS.Systems;

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
    public override void StartClientSide(ICoreClientAPI capi)
    {
        capi.ChatCommands
            .Create("gps")
            .WithDescription(LangEx.FeatureString("GPS", "ClientCommandDescription"))
            .HandleWith(OnClientDefaultHandler);
    }

    private static TextCommandResult OnClientDefaultHandler(TextCommandCallingArgs args)
    {
        var pos = PlayerLocationMessage(args.Caller.Player);
        return TextCommandResult.Success(pos);
    }
    
    private static string PlayerLocationMessage(IPlayer player)
    {
        var pos = player.Entity.Pos.AsBlockPos;
        var displayPos = pos.RelativeToSpawn();
        var message = $"X = {displayPos.X}, Y = {displayPos.Y}, Z = {displayPos.Z}.";
        return message;
    }
}