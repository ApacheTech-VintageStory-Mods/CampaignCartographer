using Gantry.Services.FileSystem.Configuration.Consumers;

namespace ApacheTech.VintageMods.CampaignCartographer.Features.Waypoints.Automatic.Patches;

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
[HarmonySidedPatch(EnumAppSide.Client)]
[SettingsConsumer(EnumAppSide.Client)]
public partial class AutoWaypointsPatches : WorldSettingsConsumer<AutoWaypointsSettings>
{
}