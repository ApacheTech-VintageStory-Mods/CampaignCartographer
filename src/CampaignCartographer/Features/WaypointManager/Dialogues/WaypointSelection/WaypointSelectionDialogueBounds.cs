using Vintagestory.Client;

namespace ApacheTech.VintageMods.CampaignCartographer.Features.WaypointManager.Dialogues.WaypointSelection;

#pragma warning disable CA1822 // Mark members as static

public class WaypointSelectionDialogueBounds
{
    public WaypointSelectionDialogueBounds()
    {
        var scaledWidth = Math.Max(600, ScreenManager.Platform.WindowSize.Width * 0.5) / ClientSettings.GUIScale;
        var scaledHeight = Math.Min(600, (ScreenManager.Platform.WindowSize.Height - 65) * 0.85) / ClientSettings.GUIScale;

        ButtonRowBoundsRightFixed = ElementBounds
            .FixedSize(60, 30)
            .WithFixedPadding(10, 2)
            .WithAlignment(EnumDialogArea.RightFixed);

        ButtonRowBounds = ElementBounds
            .FixedSize(60, 30)
            .WithFixedPadding(10, 2);

        TextBounds = ElementBounds
            .FixedSize(EnumDialogArea.CenterFixed, 0, 30)
            .WithFixedPadding(10, 2);

        HeaderBounds = ElementBounds
            .Fixed(EnumDialogArea.LeftTop, 0, 0, scaledWidth, 35);

        ContentBodyBounds = HeaderBounds
            .BelowCopy(fixedDeltaY: GapBetweenRows);

        InsetBounds = ContentBodyBounds
            .BelowCopy(0, 3)
            .WithFixedSize(scaledWidth, scaledHeight);

        ClippedBounds = InsetBounds
            .ForkContainingChild(3, 3, 3, 3);

        CellListBounds = ClippedBounds
            .ForkContainingChild(0.0, 0.0, 0.0, -3.0)
            .WithFixedPadding(10.0);
    }

    public int SwitchSize => 30;
    public double GapBetweenRows => 20;
    public ElementBounds ButtonRowBoundsRightFixed { get; }
    public ElementBounds ButtonRowBounds { get; }
    public ElementBounds TextBounds { get; }
    public ElementBounds HeaderBounds { get; }
    public ElementBounds ContentBodyBounds { get; }
    public ElementBounds InsetBounds { get; }
    public ElementBounds ClippedBounds { get; }
    public ElementBounds CellListBounds { get; }
}
