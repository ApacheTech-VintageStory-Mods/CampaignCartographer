using Gantry.Core.Contracts;
using Vintagestory.API.MathTools;

namespace ApacheTech.VintageMods.CampaignCartographer.Features.WaypointManager.Repositories.Commands;

public class AddWaypointCommand : ICommand
{
    private readonly string _command;

    public AddWaypointCommand(BlockPos position, string icon, bool pinned, string colour, string title)
    {
        _command = $"/waypoint addati {icon} {position.X} {position.Y} {position.Z} {pinned} {colour} {title}";
    }

    public void Execute()
    {
        ApiEx.Client!.SendChatMessage(_command);
    }
}