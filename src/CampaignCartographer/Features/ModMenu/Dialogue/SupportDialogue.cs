using Gantry.Core.Extensions.Helpers;
using Gantry.Core.GameContent.GUI.Abstractions;

namespace ApacheTech.VintageMods.CampaignCartographer.Features.ModMenu.Dialogue;

/// <summary>
///     User interface that acts as a hub, to bring together all features within the mod.
/// </summary>
/// <seealso cref="GenericDialogue" />
[UsedImplicitly]
public sealed class SupportDialogue : RowedButtonMenuDialogue
{
    /// <summary>
    /// 	Initialises a new instance of the <see cref="SupportDialogue"/> class.
    /// </summary>
    /// <param name="capi">The client API.</param>
    public SupportDialogue(ICoreClientAPI capi) : base(capi)
    {
        ModalTransparency = 0f;
        Title = LangEx.ModTitle();
        ShowTitleBar = true;
    }

    protected override string LangEntryPrefix => "Support.Dialogue";

    /// <summary>
    ///     Composes the header for the GUI.
    /// </summary>
    /// <param name="composer">The composer.</param>
    protected override void ComposeBody(GuiComposer composer)
    {
        AddButton(composer, LangEntry("Patreon"), () => BrowserEx.TryOpenUrl("https://www.patreon.com/ApacheTechSolutions?fan_landing=true"));
        AddButton(composer, LangEntry("Donate"), () => BrowserEx.TryOpenUrl("https://bit.ly/APGDonate"));
        AddButton(composer, LangEntry("Coffee"), () => BrowserEx.TryOpenUrl("https://www.buymeacoffee.com/Apache"));
        AddButton(composer, LangEntry("Twitch"), () => BrowserEx.TryOpenUrl("https://twitch.tv/ApacheGamingUK"));
        AddButton(composer, LangEntry("YouTube"), () => BrowserEx.TryOpenUrl("https://youtube.com/ApacheGamingUK"));
        AddButton(composer, LangEntry("WishList"), () => BrowserEx.TryOpenUrl("http://amzn.eu/7qvKTFu"));
        AddButton(composer, LangEntry("Website"), () => BrowserEx.TryOpenUrl("https://apachegaming.net"));
        IncrementRow();
        AddButton(composer, LangEntry("Back"), TryClose);
    }
}