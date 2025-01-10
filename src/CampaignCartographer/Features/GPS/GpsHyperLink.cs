using Vintagestory.API.MathTools;

namespace ApacheTech.VintageMods.CampaignCartographer.Features.GPS;

/// <summary>
///     Represents a GPS hyperlink, capable of converting between link text and position data
///     and providing map-related interactions.
/// </summary>
/// <remarks>
///     This class encapsulates functionality to parse, create, and execute GPS hyperlinks,
///     allowing for seamless integration with the in-game world map.
/// </remarks>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public class GpsHyperLink
{
    /// <summary>
    ///     The position associated with this GPS hyperlink.
    /// </summary>
    public BlockPos Position { get; set; }

    /// <summary>
    ///     Creates a new instance of <see cref="GpsHyperLink"/> from a link text component.
    /// </summary>
    /// <param name="link">The link text component containing the GPS hyperlink data.</param>
    /// <exception cref="ArgumentException">Thrown if the link does not use the recognised GPS protocol.</exception>
    /// <remarks>
    ///     Parses the GPS data from the link's HREF and constructs a <see cref="BlockPos"/> object.
    ///     The link must begin with the "gps://" protocol.
    /// </remarks>
    public GpsHyperLink(LinkTextComponent link)
    {
        if (!link.Href.StartsWith("gps://"))
        {
            throw new ArgumentException("GPS Link protocol not recognised.");
        }
        var data = link.Href.Replace("gps://", "");
        var array = data.Split('=');
        Position = new BlockPos(int.Parse(array[0]), int.Parse(array[1]), int.Parse(array[2]), Dimensions.NormalWorld);
    }

    /// <summary>
    ///     Creates a new instance of <see cref="GpsHyperLink"/> with a specified position.
    /// </summary>
    /// <param name="position">The block position associated with the hyperlink.</param>
    public GpsHyperLink(BlockPos position)
    {
        Position = position;
    }

    /// <summary>
    ///     Converts the GPS hyperlink to a clickable HTML string.
    /// </summary>
    /// <returns>
    ///     An HTML string representing the GPS hyperlink with a "Show On Map" label.
    /// </returns>
    /// <remarks>
    ///     The hyperlink uses the "gps://" protocol and includes the position data
    ///     in the format "X=Y=Z".
    /// </remarks>
    public string ToHyperlink()
    {
        var path = $"{Position.X}={Position.Y}={Position.Z}";
        return $"<a href=\"gps://{path}\">{LangEx.FeatureString("GPS", "ShowOnMap")}</a>";
    }

    /// <summary>
    ///     Executes the action associated with a GPS hyperlink.
    /// </summary>
    /// <param name="link">The link text component containing the GPS hyperlink data.</param>
    /// <remarks>
    ///     This static method parses the GPS hyperlink data and centres the world map
    ///     to the corresponding position.
    /// </remarks>
    public static void Execute(LinkTextComponent link)
    {
        new GpsHyperLink(link).Execute();
    }

    /// <summary>
    ///     Executes the GPS hyperlink action, centring the map on the associated position.
    /// </summary>
    /// <remarks>
    ///     This method toggles the world map to the appropriate dialog mode, if necessary,
    ///     and centres the map to the position associated with this instance.
    /// </remarks>
    public void Execute()
    {
        var mapManager = IOC.Services.Resolve<WorldMapManager>();

        if (mapManager.worldMapDlg is null ||
            !mapManager.worldMapDlg.IsOpened() ||
            mapManager.worldMapDlg.IsOpened() && mapManager.worldMapDlg.DialogType == EnumDialogType.HUD)
        {
            mapManager.ToggleMap(EnumDialogType.Dialog);
        }

        var map = mapManager.worldMapDlg?.SingleComposer.GetElement("mapElem") as GuiElementMap;
        map?.CenterMapTo(Position);
    }
}