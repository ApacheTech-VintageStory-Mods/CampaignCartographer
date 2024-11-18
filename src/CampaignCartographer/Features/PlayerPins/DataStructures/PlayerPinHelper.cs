﻿using System.Drawing;
using Gantry.Services.FileSystem.Configuration.Consumers;

namespace ApacheTech.VintageMods.CampaignCartographer.Features.PlayerPins.DataStructures;

/// <summary>
///     A helper class that eases the retrieval and setting of pin values, based on an entity's relationship to the current client player.
/// </summary>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
[SettingsConsumer(EnumAppSide.Client)]
public sealed class PlayerPinHelper : WorldSettingsConsumer<PlayerPinsSettings>
{
    /// <summary>
    ///     Gets or sets the relation.
    /// </summary>
    /// <value>The relation.</value>
    public static PlayerRelation Relation { get; set; } = PlayerRelation.Self;

    public static Color Colour
    {
        get
        {
            return Relation switch
            {
                PlayerRelation.Self => Settings.SelfColour,
                PlayerRelation.Highlighted => Settings.HighlightColour,
                PlayerRelation.Others => Settings.OthersColour,
                _ => throw new ArgumentOutOfRangeException()
            };
        }
        set
        {
            switch (Relation)
            {
                case PlayerRelation.Self:
                    Settings.SelfColour = value;
                    break;
                case PlayerRelation.Highlighted:
                    Settings.HighlightColour = value;
                    break;
                case PlayerRelation.Others:
                    Settings.OthersColour = value;
                    break;
                default: throw new ArgumentOutOfRangeException(nameof(value));
            }
        }
    }

    public static int Scale
    {
        get
        {
            return Relation switch
            {
                PlayerRelation.Self => Settings.SelfScale,
                PlayerRelation.Highlighted => Settings.HighlightScale,
                PlayerRelation.Others => Settings.OthersScale,
                _ => throw new ArgumentOutOfRangeException()
            };
        }
        set
        {
            switch (Relation)
            {
                case PlayerRelation.Self:
                    Settings.SelfScale = value;
                    break;
                case PlayerRelation.Highlighted:
                    Settings.HighlightScale = value;
                    break;
                case PlayerRelation.Others:
                    Settings.OthersScale = value;
                    break;
                default: throw new ArgumentOutOfRangeException(nameof(value));
            }
        }
    }
}