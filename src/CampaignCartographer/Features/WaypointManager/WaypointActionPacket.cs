using Gantry.Core.GameContent.GUI.Abstractions;

namespace ApacheTech.VintageMods.CampaignCartographer.Features.WaypointManager;

/// <summary>
///     Represents a network packet for performing actions on a waypoint.
/// </summary>
[ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
public class WaypointActionPacket
{
    /// <summary>
    ///     The waypoint associated with the action.
    /// </summary>
    public Waypoint Waypoint { get; set; }

    /// <summary>
    ///     The mode indicating whether the action is an addition or an edit.
    /// </summary>
    public CrudAction Mode { get; set; }
}
