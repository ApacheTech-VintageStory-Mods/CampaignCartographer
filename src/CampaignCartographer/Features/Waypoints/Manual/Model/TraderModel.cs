﻿using Gantry.Services.FileSystem.Abstractions.Contracts;

namespace ApacheTech.VintageMods.CampaignCartographer.Features.Waypoints.Manual.Model;

/// <summary>
///     Represents meta date information for trader waypoints.
/// </summary>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public sealed class TraderModel
{
    /// <summary>
    ///     Gets the colour associated with the specified trader.
    /// </summary>
    /// <param name="trader">The trader to find the colour of.</param>
    /// <returns>A <see cref="string"/> representation of the colour to use for the waypoint of the trader.</returns>
    public static string GetColourFor(EntityTrader trader)
    {
        var colours = IOC.Services.Resolve<IFileSystemService>()
            .GetJsonFile("trader-colours.json")
            .ParseAs<Dictionary<string, string>>();

        return colours.SingleOrDefault(p =>
                trader.Code.Path
                    .ToLowerInvariant()
                    .EndsWith(p.Key))
            .Value ?? colours["default"];
    }
}