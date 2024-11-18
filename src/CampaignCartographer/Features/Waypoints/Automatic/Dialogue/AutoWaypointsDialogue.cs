using Gantry.Services.FileSystem.Dialogue;

namespace ApacheTech.VintageMods.CampaignCartographer.Features.Waypoints.Automatic.Dialogue;

/// <summary>
///     APL for the AutoWaypoints feature.
/// </summary>
/// <seealso cref="AutomaticFeatureSettingsDialogue{TFeatureSettings}" />
/// <seealso cref="FeatureSettingsDialogue{AutoWaypointsSettings}" />
/// <seealso cref="GuiDialog" />
[UsedImplicitly]
public sealed class AutoWaypointsDialogue : AutomaticFeatureSettingsDialogue<AutoWaypointsSettings>
{
    public AutoWaypointsDialogue(ICoreClientAPI capi, AutoWaypointsSettings settings) : base(capi, settings, "AutoWaypoints")
    {
        // Everything is set up procedurally, within the base class.
    }
}