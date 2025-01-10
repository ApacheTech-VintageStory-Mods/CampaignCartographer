using Gantry.Core.Contracts;
using Vintagestory.API.MathTools;

namespace ApacheTech.VintageMods.CampaignCartographer.Features.WaypointManager.Repositories.Commands;

/// <summary>
///     Represents a command to add a new waypoint at a specified position.
/// </summary>
public class AddWaypointCommand : ICommand
{
    private readonly string _command;

    /// <summary>
    ///     Initialises a new instance of the <see cref="AddWaypointCommand"/> class with explicit parameters.
    /// </summary>
    /// <param name="position">The position where the waypoint will be added.</param>
    /// <param name="icon">The icon to be associated with the waypoint.</param>
    /// <param name="pinned">Indicates whether the waypoint should be pinned.</param>
    /// <param name="colour">The colour of the waypoint, in hexadecimal format.</param>
    /// <param name="title">The title of the waypoint.</param>
    public AddWaypointCommand(BlockPos position, string icon, bool pinned, string colour, string title)
    {
        _command = $"/waypoint addati {icon} ={position.X} ={position.Y} ={position.Z} {pinned} {colour} {title}";
    }

    /// <summary>
    ///     Initialises a new instance of the <see cref="AddWaypointCommand"/> class using a <see cref="Waypoint"/>.
    /// </summary>
    /// <param name="position">The position where the waypoint will be added.</param>
    /// <param name="waypoint">The <see cref="Waypoint"/> object containing the details of the waypoint.</param>
    public AddWaypointCommand(BlockPos position, Waypoint waypoint)
        : this(position, waypoint.Icon, waypoint.Pinned, ColorUtil.Int2Hex(waypoint.Color), waypoint.Title)
    {
    }

    /// <summary>
    ///     Executes the command to add the waypoint.
    /// </summary>
    public void Execute()
    {
        ApiEx.Client.SendChatMessage(_command);
    }
}