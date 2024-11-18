using Gantry.Core.Contracts;

namespace ApacheTech.VintageMods.CampaignCartographer.Features.WaypointManager.Repositories.Commands;

public class RemoveWaypointCommand : ICommand
{
    private readonly string _command;

    public RemoveWaypointCommand(int index)
    {
        _command = $"/waypoint remove {index}";
    }

    public void Execute()
    {
        ApiEx.Client!.SendChatMessage(_command);
    }
}