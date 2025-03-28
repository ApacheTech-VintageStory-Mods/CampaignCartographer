using Vintagestory.API.Util;

namespace ApacheTech.VintageMods.CampaignCartographer.Features.ManualWaypoints.Model;

/// <summary>
///     Represents an icon that can be used to mark a waypoint on the world map.
/// </summary>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public partial class WaypointIconModel
{
    /// <summary>
    /// 	Initialises a new instance of the <see cref="WaypointIconModel"/> class.
    /// </summary>
    /// <param name="name">The name.</param>
    /// <param name="isVanilla">if set to <c>true</c> [is vanilla].</param>
    public WaypointIconModel(string name, bool isVanilla = false)
    {
        Name = name.ToLowerInvariant();
        DisplayName = name.UcFirst().SplitPascalCase();
        Glyph = $"<icon name=\"{name}\">";
        IsVanilla = isVanilla;
    }

    /// <summary>
    ///     Gets or sets the internal name for the icon.
    /// </summary>
    /// <value>The internal name for the icon.</value>
    public string Name { get; set; }

    /// <summary>
    ///     Gets or sets the displayed name for the icon.
    /// </summary>
    /// <value>The displayed name for the icon.</value>
    public string DisplayName { get; set; }

    /// <summary>
    ///     Gets or sets the glyph that is displayed.
    ///
    ///     Example: &lt;icon name="{name}"&gt;
    /// </summary>
    /// <value>The glyph.</value>
    public string Glyph { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating whether this icon is vanilla.
    /// </summary>
    /// <value><c>true</c> if this icon is vanilla; otherwise, <c>false</c>.</value>
    public bool IsVanilla { get; set; }

    /// <summary>
    ///     Gets a list of the vanilla icons.
    /// </summary>
    public static IEnumerable<WaypointIconModel> GetVanillaIcons() =>
        IOC.Services.GetRequiredService<WaypointMapLayer>().WaypointIcons.Select(p => new WaypointIconModel(p.Key, true));
}