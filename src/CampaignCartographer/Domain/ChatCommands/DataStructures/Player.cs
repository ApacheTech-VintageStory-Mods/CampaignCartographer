namespace ApacheTech.VintageMods.CampaignCartographer.Domain.ChatCommands.DataStructures;

/// <summary>
///     Represents a player that's been added to a whitelist, or blacklist.
/// </summary>
[JsonObject]
public record Player(string Id, string Name);