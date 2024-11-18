using Gantry.Services.FileSystem.Configuration.Consumers;

namespace ApacheTech.VintageMods.CampaignCartographer.Features.PlayerPins.Patches;

[HarmonySidedPatch(EnumAppSide.Client)]
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
[SettingsConsumer(EnumAppSide.Client)]
public partial class PlayerPinsPatches : WorldSettingsConsumer<PlayerPinsSettings>
{
}