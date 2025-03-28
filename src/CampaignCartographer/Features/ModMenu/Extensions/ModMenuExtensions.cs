using System.Diagnostics.CodeAnalysis;
using Gantry.Core.GameContent.GUI.Abstractions;

namespace ApacheTech.VintageMods.CampaignCartographer.Features.ModMenu.Extensions;

/// <summary>
///     Extension methods to aid registration of hub dialogues.
/// </summary>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public static class ModMenuExtensions
{
    /// <summary>
    ///     Adds an accessibility hub dialogue.
    /// </summary>
    [SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Extension Method")]
    public static void AddModMenuDialogue<T>(this ICoreClientAPI capi, string title) where T : GenericDialogue
    {
        IOC.Services.GetRequiredService<ModMenuClientSystem>().FeatureDialogues
            .AddIfNotPresent(typeof(T), LangEx.FeatureString($"{title}.Dialogue", "Title"));
    }
}