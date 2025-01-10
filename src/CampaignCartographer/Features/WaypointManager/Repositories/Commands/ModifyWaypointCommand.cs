using Gantry.Core.Contracts;
using Vintagestory.API.MathTools;

namespace ApacheTech.VintageMods.CampaignCartographer.Features.WaypointManager.Repositories.Commands;

/// <summary>
///     Represents a command to modify the properties of a waypoint.
/// </summary>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public class ModifyWaypointCommand : ICommand
{
    private readonly string _command;

    /// <summary>
    ///     Initialises a new instance of the <see cref="ModifyWaypointCommand"/> class with explicit parameters.
    /// </summary>
    /// <param name="index">The index of the waypoint to be modified.</param>
    /// <param name="icon">The new icon for the waypoint.</param>
    /// <param name="pinned">Indicates whether the waypoint should be pinned.</param>
    /// <param name="colour">The new colour for the waypoint, in hexadecimal format.</param>
    /// <param name="title">The new title for the waypoint.</param>
    public ModifyWaypointCommand(int index, string icon, bool pinned, string colour, string title)
    {
        _command = $"/waypoint modify {index} {colour} {icon} {pinned} {title}";
    }

    /// <summary>
    ///     Initialises a new instance of the <see cref="ModifyWaypointCommand"/> class using a <see cref="Waypoint"/>.
    /// </summary>
    /// <param name="index">The index of the waypoint to be modified.</param>
    /// <param name="waypoint">The <see cref="Waypoint"/> object containing the new properties.</param>
    public ModifyWaypointCommand(int index, Waypoint waypoint)
        : this(index, waypoint.Icon, waypoint.Pinned, ColorUtil.Int2Hex(waypoint.Color), waypoint.Title)
    {
    }

    /// <summary>
    ///     Executes the command to modify the waypoint.
    /// </summary>
    public void Execute() => ApiEx.Client.SendChatMessage(_command);
}