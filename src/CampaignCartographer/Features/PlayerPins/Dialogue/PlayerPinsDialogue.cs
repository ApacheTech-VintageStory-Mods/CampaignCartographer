 using ApacheTech.Common.Extensions.Enum;
using ApacheTech.Common.Extensions.Harmony;
using ApacheTech.VintageMods.CampaignCartographer.Features.PlayerPins.DataStructures;
using Cairo;
using Gantry.Core.Hosting.Annotation;
using Gantry.Services.FileSystem.Dialogue;

using Color = System.Drawing.Color;

namespace ApacheTech.VintageMods.CampaignCartographer.Features.PlayerPins.Dialogue;

/// <summary>
///     GUI Window: Player Pins Settings.
/// </summary>
/// <seealso cref="FeatureSettingsDialogue{TFeatureSettings}" />
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public class PlayerPinsDialogue : FeatureSettingsDialogue<PlayerPinsSettings>
{
    private readonly PlayerPins _playerPins;

    [SidedConstructor(EnumAppSide.Client)]
    public PlayerPinsDialogue(ICoreClientAPI capi, PlayerPinsSettings settings)
        : base(capi, settings, "PlayerPins")
    {
        Movable = true;
        _playerPins = capi.ModLoader.GetModSystem<PlayerPins>();
    }

    private void RefreshTextures()
    {
        _playerPins.LoadTextures();

        capi.ModLoader
            .GetModSystem<WorldMapManager>()
            .PlayerMapLayer()
            .OnMapOpenedClient();
    }

    protected override void RefreshValues()
    {
        if (!IsOpened()) return;

        var colour = PlayerPinHelper.Colour;

        SetPlayerPinSwitch("btnTogglePlayerPins", (int)PlayerPinHelper.Relation);
        SetColourSliderValue("sliderR", colour.R);
        SetColourSliderValue("sliderG", colour.G);
        SetColourSliderValue("sliderB", colour.B);
        SetColourSliderValue("sliderA", colour.A);
        SetScaleSliderValue("sliderScale", PlayerPinHelper.Scale);
        SetPreviewColour("pnlPreview");
    }

    protected override void ComposeBody(GuiComposer composer)
    {
        const int switchSize = 20;
        const int switchPadding = 20;
        const double sliderWidth = 200.0;
        var font = CairoFont.WhiteSmallText();

        var names = new[]
        {
            T("Self"),
            T("Highlighted"),
            T("Others")
        };
        var values = new[] { "Self", "Highlighted", "Others" };

        var sliderBounds = ElementBounds.Fixed(160, GuiStyle.TitleBarHeight, switchSize, switchSize);
        var textBounds = ElementBounds.Fixed(0, GuiStyle.TitleBarHeight + 1.0, 150, switchSize);

        var bounds = sliderBounds.FlatCopy().WithFixedWidth(sliderWidth).WithFixedHeight(GuiStyle.TitleBarHeight + 1.0);
        composer.AddStaticText(T("lblToggleSwitch"), font, textBounds);
        composer.AddHoverText(T("lblToggleSwitch.HoverText"), font, 260, textBounds);
        composer.AddDropDown(values, names, 0, OnSelectionChanged, bounds, font, "btnTogglePlayerPins");

        textBounds = textBounds.BelowCopy(fixedDeltaY: switchPadding + 5);
        sliderBounds = sliderBounds.BelowCopy(fixedDeltaY: switchPadding + 5);
        composer.AddStaticText(T("lblRed"), font, textBounds);
        composer.AddHoverText(T("lblRed.HoverText"), font, 260, textBounds);
        composer.AddLazySlider(OnRChanged, sliderBounds.FlatCopy().WithFixedWidth(sliderWidth), "sliderR");

        textBounds = textBounds.BelowCopy(fixedDeltaY: switchPadding);
        sliderBounds = sliderBounds.BelowCopy(fixedDeltaY: switchPadding);
        composer.AddStaticText(T("lblGreen"), font, textBounds);
        composer.AddHoverText(T("lblGreen.HoverText"), font, 260, textBounds);
        composer.AddLazySlider(OnGChanged, sliderBounds.FlatCopy().WithFixedWidth(sliderWidth), "sliderG");

        textBounds = textBounds.BelowCopy(fixedDeltaY: switchPadding);
        sliderBounds = sliderBounds.BelowCopy(fixedDeltaY: switchPadding);
        composer.AddStaticText(T("lblBlue"), font, textBounds);
        composer.AddHoverText(T("lblBlue.HoverText"), font, 260, textBounds);
        composer.AddLazySlider(OnBChanged, sliderBounds.FlatCopy().WithFixedWidth(sliderWidth), "sliderB");

        textBounds = textBounds.BelowCopy(fixedDeltaY: switchPadding);
        sliderBounds = sliderBounds.BelowCopy(fixedDeltaY: switchPadding);
        composer.AddStaticText(T("lblOpacity"), font, textBounds);
        composer.AddHoverText(T("lblOpacity.HoverText"), font, 260, textBounds);
        composer.AddLazySlider(OnAChanged, sliderBounds.FlatCopy().WithFixedWidth(sliderWidth), "sliderA");

        textBounds = textBounds.BelowCopy(fixedDeltaY: switchPadding);
        sliderBounds = sliderBounds.BelowCopy(fixedDeltaY: switchPadding);
        composer.AddStaticText(T("lblScale"), font, textBounds);
        composer.AddHoverText(T("lblScale.HoverText"), font, 260, textBounds);
        composer.AddLazySlider(OnScaleChanged, sliderBounds.FlatCopy().WithFixedWidth(sliderWidth), "sliderScale");

        textBounds = textBounds.BelowCopy(fixedDeltaY: switchPadding);
        composer.AddDynamicCustomDraw(textBounds.FlatCopy().WithFixedWidth(textBounds.fixedWidth + sliderWidth + 10), OnPreviewPanelDraw, "pnlPreview");

        textBounds = textBounds.BelowCopy(fixedDeltaY: switchPadding);
        composer.AddSmallButton(T("Randomise"), OnRandomise,
            textBounds.FlatCopy().WithFixedWidth(360).WithFixedHeight(GuiStyle.TitleBarHeight + 1.0));

        SingleComposer = composer.EndChildElements().Compose();
    }

    public override bool TryClose()
    {
        RefreshTextures();
        return base.TryClose();
    }

    private void OnSelectionChanged(string code, bool selected)
    {
        PlayerPinHelper.Relation = Enum.TryParse(code, out PlayerRelation relation) ? relation : PlayerRelation.Self;
        RefreshTextures();
        RefreshValues();
    }

    #region Set GUI Values

    private void SetColourSliderValue(string name, int value)
    {
        SingleComposer.GetSlider(name).SetValues(value, 0, 255, 1);
    }

    private void SetScaleSliderValue(string name, int value)
    {
        SingleComposer.GetSlider(name).SetValues(value, -5, 20, 1);
    }

    private void SetPlayerPinSwitch(string name, int value)
    {
        SingleComposer.GetDropDown(name).SetSelectedIndex(value);
    }

    private void SetPreviewColour(string name)
    {
        SingleComposer.GetCustomDraw(name).Redraw();
    }

    #endregion

    #region GUI Business Logic Callbacks

    private bool OnRandomise()
    {
        var rng = new Random(DateTime.Now.Millisecond);
        PlayerPinHelper.Colour = Color.FromArgb(rng.Next(0, 256), rng.Next(0, 256), rng.Next(0, 256), rng.Next(0, 256));
        PlayerPinHelper.Scale = rng.Next(-5, 21);
        RefreshTextures();
        RefreshValues();
        return true;
    }

    private static void OnPreviewPanelDraw(Context ctx, ImageSurface surface, ElementBounds currentBounds)
    {
        var colour = PlayerPinHelper.Colour.ToNormalisedRgba();

        ctx.SetSourceRGBA(0, 0, 0, 1);
        ctx.LineWidth = 5.0;
        ctx.Rectangle(0, 0, surface.Width, surface.Height);
        ctx.Stroke();

        ctx.SetSourceRGBA(colour[0], colour[1], colour[2], colour[3]);
        ctx.Rectangle(2, 2, surface.Width - 4, surface.Height - 4);
        ctx.Fill();
    }

    private bool OnAChanged(int a) => OnColourChanged(ColourChannel.A, a);

    private bool OnRChanged(int r) => OnColourChanged(ColourChannel.R, r);

    private bool OnGChanged(int g) => OnColourChanged(ColourChannel.G, g);

    private bool OnBChanged(int b) => OnColourChanged(ColourChannel.B, b);

    private bool OnColourChanged(ColourChannel channel, int value)
    {
        PlayerPinHelper.Colour = PlayerPinHelper.Colour.UpdateColourChannel(channel, (byte)value);
        RefreshTextures();
        RefreshValues();
        return true;
    }

    private bool OnScaleChanged(int s)
    {
        PlayerPinHelper.Scale = s;
        RefreshTextures();
        RefreshValues();
        return true;
    }

    #endregion
}