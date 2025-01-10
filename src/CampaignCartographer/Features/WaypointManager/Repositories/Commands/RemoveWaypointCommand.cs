using Gantry.Core.Contracts;

namespace ApacheTech.VintageMods.CampaignCartographer.Features.WaypointManager.Repositories.Commands;

/// <summary>
///     Represents a command to remove a waypoint by its index.
/// </summary>
/// <param name="index">The index of the waypoint to be removed.</param>
public class RemoveWaypointCommand(int index) : ICommand
{
    private readonly string _command = $"/waypoint remove {index}";

    /// <summary>
    ///     Executes the command to remove the specified waypoint.
    /// </summary>
    public void Execute() => ApiEx.Client.SendChatMessage(_command);
}