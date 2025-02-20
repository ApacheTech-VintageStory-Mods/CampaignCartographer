using System.Drawing;
using ApacheTech.Common.FunctionalCSharp.Extensions;
using ApacheTech.VintageMods.CampaignCartographer.Features.WaypointManager.WaypointTemplates;
using Gantry.Core.Extensions.Api;
using Vintagestory.API.MathTools;

namespace ApacheTech.VintageMods.CampaignCartographer.Features.WaypointManager.Extensions;

/// <summary>
///     Provides extension methods for waypoint templates.
/// </summary>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public static class WaypointTemplateExtensions
{
    private static readonly List<BlockPos> _positionsBeingHandled = [];

    /// <summary>
    ///     Adds a <see cref="CoverageWaypointTemplate"/> to the world map. These are waypoints that haven't been added before.
    /// </summary>
    /// <param name="waypoint">The waypoint.</param>
    /// <param name="position">The position.</param>
    /// <param name="force"></param>
    public static void AddToMap(this CoverageWaypointTemplate waypoint, BlockPos position = null, bool force = false)
    {
        // DEV NOTE:    This method looks needlessly complicated because of race conditions when running in single-player mode.
        //              In these cases, it's possible for this method to be run multiple times, resulting in multiple waypoints
        //              being added. This was intended as a temporary fix, however, it seems like without adding a global cooldown
        //              for waypoint related actions, this is the best way to resolve the issues. This still enables waypoints
        //              to be added in rapid succession, but limits the calls to one pass-through per block, per second.

        position ??= ApiEx.Client.World.Player.Entity.Pos.AsBlockPos;
        if (_positionsBeingHandled.Contains(position)) return;
        _positionsBeingHandled.Add(position);
        try
        {
            if (!force && position.WaypointExistsWithinRadius(waypoint.HorizontalCoverageRadius, waypoint.VerticalCoverageRadius,
                    p =>
                    {
                        var sameIcons = p.Icon.EndsWith(waypoint.DisplayedIcon, StringComparison.InvariantCultureIgnoreCase);
                        var sameColour = p.Color == waypoint.Colour.ColourValue();
                        return sameIcons && sameColour;
                    })) return;
            ApiEx.ClientMain.EnqueueMainThreadTask(() =>
            {
                ApiEx.ClientMain.AddWaypointAtPos(position, waypoint.DisplayedIcon.ToLower(), waypoint.Colour.ToLower(), waypoint.Title, waypoint.Pinned);
            }, "");
        }
        finally
        {
            ApiEx.Client.RegisterDelayedCallback(_ => { _positionsBeingHandled.Remove(position); }, 1000);
        }
    }

    /// <summary>
    ///     Adds a <see cref="WaypointTemplateBase"/> to the world map. These are waypoints that are being imported into the game, and have a position already.
    /// </summary>
    /// <param name="waypoint">The waypoint to add.</param>
    /// <param name="position">The position to add the waypoint at.</param>
    public static void AddToMap(this WaypointTemplateBase waypoint, BlockPos position)
    {
        // DEV NOTE:    This method looks needlessly complicated because of race conditions when running in single-player mode.
        //              In these cases, it's possible for this method to be run multiple times, resulting in multiple waypoints
        //              being added. This was intended as a temporary fix, however, it seems like without adding a global cooldown
        //              for waypoint related actions, this is the best way to resolve the issues. This still enables waypoints
        //              to be added in rapid succession, but limits the calls to one pass-through per block, per second.

        if (_positionsBeingHandled.Contains(position)) return;
        _positionsBeingHandled.Add(position);
        try
        {
            ApiEx.ClientMain.EnqueueMainThreadTask(() =>
            {
                ApiEx.ClientMain.AddWaypointAtPos(position, waypoint.ServerIcon.ToLower(), waypoint.Colour.ToLower(), waypoint.Title, waypoint.Pinned);
            }, "");
        }
        finally
        {
            ApiEx.Client.RegisterDelayedCallback(_ => { _positionsBeingHandled.Remove(position); }, 1000);
        }
    }

    /// <summary>
    ///     Converts a waypoint template to a waypoint that can be added to the map.
    /// </summary>
    /// <param name="template">The template.</param>
    /// <returns></returns>
    public static Waypoint ToWaypoint(this WaypointTemplateBase template)
    {
        return new Waypoint
        {
            Guid = template.Id,
            Title = template.Title,
            Color = template.Colour.ToInt(),
            Pinned = template.Pinned,
            Icon = template.ServerIcon,
            Temporary = false
        };
    }

    /// <summary>
    ///     Converts a waypoint template to a waypoint that can be added to the map.
    /// </summary>
    /// <param name="template">The template.</param>
    /// <returns></returns>
    public static Waypoint ToWaypoint(this PositionedWaypointTemplate template)
    {
        return new Waypoint
        {
            Guid = template.Id,
            Position = template.Position,
            Title = template.Title,
            Color = template.Colour.ToInt(),
            Pinned = template.Pinned,
            Icon = template.ServerIcon,
            Temporary = false
        };
    }

    /// <summary>
    ///     Compares two waypoint templates to determine if they are the same based on specific properties.
    /// </summary>
    /// <param name="this">The current waypoint template.</param>
    /// <param name="other">The other waypoint template to compare to.</param>
    /// <returns>True if the waypoint templates have the same title, colour, pinned state, and server icon; otherwise, false.</returns>
    public static bool IsSameAs(this WaypointTemplateBase @this, WaypointTemplateBase other)
    {
        return @this.Validate(
            x => x.Title == other.Title,
            x => x.Colour.ToInt() == other.Colour.ToInt(),
            x => x.Pinned == other.Pinned,
            x => x.ServerIcon == other.ServerIcon);
    }

    /// <summary>
    ///     Converts a string representing a colour to its integer value.
    /// </summary>
    /// <param name="colourString">The colour string to convert.</param>
    /// <returns>The integer representation of the colour.</returns>
    public static int ToInt(this string colourString) 
        => (colourString.StartsWith("#")
            ? ColorUtil.Hex2Int(colourString)
            : ColorUtil.Hex2Int(Color.FromName(colourString).ToRgbaHexString())) | -16777216;

    /// <summary>
    ///     Converts an integer representing a colour to its hexadecimal string representation.
    /// </summary>
    /// <param name="intColour">The integer value of the colour.</param>
    /// <returns>The hexadecimal string representation of the colour.</returns>
    public static string ToHexString(this int intColour)
        => ColorUtil.Int2Hex(intColour);
}