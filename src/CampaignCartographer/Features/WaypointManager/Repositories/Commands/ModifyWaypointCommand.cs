using Gantry.Core.Contracts;

namespace ApacheTech.VintageMods.CampaignCartographer.Features.WaypointManager.Repositories.Commands;

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public class ModifyWaypointCommand : ICommand
{
    private readonly string _command;

    public ModifyWaypointCommand(int index, string icon, bool pinned, string colour, string title)
    {
        _command = $"/waypoint modify {index} {colour} {icon} {pinned} {title}";
    }

    public void Execute()
    {
        ApiEx.Client!.SendChatMessage(_command);
    }
}