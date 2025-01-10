using Gantry.Services.FileSystem.Dialogue;

namespace ApacheTech.VintageMods.CampaignCartographer.Features.AutoWaypoints.Dialogue;

/// <summary>
///     APL for the AutoWaypoints feature.
/// </summary>
/// <seealso cref="AutomaticFeatureSettingsDialogue{TFeatureSettings}" />
/// <seealso cref="FeatureSettingsDialogue{AutoWaypointsSettings}" />
/// <seealso cref="GuiDialog" />
[UsedImplicitly]
public sealed class AutoWaypointsDialogue(ICoreClientAPI capi, AutoWaypointsSettings settings)
    : AutomaticFeatureSettingsDialogue<AutoWaypointsSettings>(capi, settings, "AutoWaypoints");