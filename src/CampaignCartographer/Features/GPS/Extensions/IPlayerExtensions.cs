namespace ApacheTech.VintageMods.CampaignCartographer.Features.GPS.Extensions;

/// <summary>
///     Provides extension methods for the <see cref="IPlayer"/> interface.
/// </summary>
/// <remarks>
///     This class contains utility methods to enhance the functionality of <see cref="IPlayer"/>, 
///     such as retrieving a player's GPS location in a formatted string.
/// </remarks>
public static class IPlayerExtensions
{
    /// <summary>
    ///     Retrieves the GPS location of the player as a formatted string.
    /// </summary>
    /// <param name="player">The player whose location is to be retrieved.</param>
    /// <returns>A string containing the player's X, Y, and Z coordinates relative to the spawn point.</returns>
    /// <remarks>
    ///     The returned string provides the player's position in the format:
    ///     "X = [value], Y = [value], Z = [value]."
    ///     The coordinates are calculated relative to the world's spawn point.
    /// </remarks>
    public static string GpsLocation(this IPlayer player)
    {
        var pos = player.Entity.Pos.AsBlockPos;
        var displayPos = pos.RelativeToSpawn();
        var message = $"X = {displayPos.X}, Y = {displayPos.Y}, Z = {displayPos.Z}.";
        return message;
    }
}