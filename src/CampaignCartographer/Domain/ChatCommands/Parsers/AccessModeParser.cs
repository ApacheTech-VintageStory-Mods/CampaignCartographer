using System.Diagnostics.Contracts;
using System.Globalization;
using ApacheTech.VintageMods.CampaignCartographer.Domain.ChatCommands.DataStructures;

namespace ApacheTech.VintageMods.CampaignCartographer.Domain.ChatCommands.Parsers;

/// <summary>
///     Parses a string as a <see cref="AccessMode"/> value, allowing partial matches.
/// </summary>
/// <seealso cref="ArgumentParserBase" />
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal class AccessModeParser : ArgumentParserBase
{
    /// <summary>
    ///     Initialises a new instance of the <see cref="AccessModeParser"/> class.
    /// </summary>
    public AccessModeParser(string argName, bool isMandatoryArg) : base(argName, isMandatoryArg)
    {
    }

    /// <summary>
    ///     The mode of access for specific feature.
    /// </summary>
    public AccessMode? Mode { get; private set; }

    /// <inheritdoc />
    public override EnumParseResult TryProcess(TextCommandCallingArgs args, Action<AsyncParseResults> onReady = null)
    {
        var value = args.RawArgs.PopWord("");
        Mode = DirectParse(value) ?? FuzzyParse(value);
        return Mode is null
            ? EnumParseResult.Bad
            : EnumParseResult.Good;
    }

    /// <inheritdoc />
    public override object GetValue() => Mode;

    /// <inheritdoc />
    public override void SetValue(object data)
    {
        Mode = data switch
        {
            int index => (AccessMode)index,
            string value => DirectParse(value) ?? FuzzyParse(value),
            AccessMode mode => mode,
            _ => null
        };
    }

    [Pure]
    private static AccessMode? DirectParse(string value)
    {
        return Enum.TryParse(value, true, out AccessMode result)
            ? result
            : null;
    }

    [Pure]
    private static AccessMode? FuzzyParse(string value)
    {
        return value switch
        {
            _ when "disabled".StartsWith(value, true, CultureInfo.InvariantCulture) => AccessMode.Disabled,
            _ when "enabled".StartsWith(value, true, CultureInfo.InvariantCulture) => AccessMode.Enabled,
            _ when "whitelist".StartsWith(value, true, CultureInfo.InvariantCulture) => AccessMode.Whitelist,
            _ when "blacklist".StartsWith(value, true, CultureInfo.InvariantCulture) => AccessMode.Blacklist,
            _ when value.Equals("wl", StringComparison.InvariantCultureIgnoreCase) => AccessMode.Whitelist,
            _ => null,
        };
    }
}