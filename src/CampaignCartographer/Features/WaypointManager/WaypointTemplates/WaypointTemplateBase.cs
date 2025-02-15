using Gantry.Core.GameContent.AssetEnum;

// ReSharper disable NonReadonlyMemberInGetHashCode

namespace ApacheTech.VintageMods.CampaignCartographer.Features.WaypointManager.WaypointTemplates;

/// <summary>
///     Represents a Waypoint, that can be added the map within the game.
/// </summary>
[JsonObject]
[ProtoContract]
public class WaypointTemplateBase : ICloneable, IEquatable<WaypointTemplateBase>
{
    protected string _title = string.Empty;

    /// <summary>
    ///     Gets or sets the title of the waypoint.
    /// </summary>
    /// <value>The title of the waypoint.</value>
    [ProtoMember(1)]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the title of the waypoint.
    /// </summary>
    /// <value>The title of the waypoint.</value>
    [JsonRequired]
    [ProtoMember(2)]
    public string Title { get => _title; set => _title = value; }
    /// <summary>
    ///     Gets or sets the icon that will be saved to the server.
    /// </summary>
    /// <value>The icon that will be saved to the server.</value>
    [JsonRequired]
    [ProtoMember(3)]
    public string ServerIcon { get; set; } = WaypointIcon.Circle;

    /// <summary>
    ///     Gets or sets the icon that will be displayed on the map.
    /// </summary>
    /// <value>The icon that will be displayed on the map.</value>
    [JsonRequired]
    [ProtoMember(4)]
    public string DisplayedIcon { get; set; } = WaypointIcon.Circle;

    /// <summary>
    ///     Gets or sets the colour of the icon to be displayed.
    /// </summary>
    /// <value>The colour of the icon to be displayed.</value>
    [JsonRequired]
    [ProtoMember(5)]
    public string Colour { get; set; } = NamedColour.Black;

    /// <summary>
    ///     Gets or sets a value indicating whether this waypoint is pinned to the map, so that
    ///     it is still rendered when the screen caret is not focussed on the map region.
    /// </summary>
    /// <value><c>true</c> if pinned; otherwise, <c>false</c>.</value>
    [JsonRequired]
    [ProtoMember(6)]
    public bool Pinned { get; set; }

    /// <summary>
    ///     Indicates whether the current object is equal to another object of the same type.
    /// </summary>
    /// <param name="other">An object to compare with this object.</param>
    /// <returns>
    ///     true if the current object is equal to the <paramref name="other">other</paramref> parameter; otherwise, false.
    /// </returns>
    public bool Equals(WaypointTemplateBase other) => Id.Equals(other?.Id, StringComparison.InvariantCultureIgnoreCase);

    /// <summary>
    ///     Determines whether the specified <see cref="object" />, is equal to this instance.
    /// </summary>
    /// <param name="obj">The <see cref="object" /> to compare with this instance.</param>
    /// <returns>
    ///     <c>true</c> if the specified <see cref="object" /> is equal to this instance; otherwise, <c>false</c>.
    /// </returns>
    public override bool Equals(object obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        return obj.GetType() == GetType() && Equals(obj.To<WaypointTemplateBase>());
    }

    /// <summary>
    ///     Returns a hash code for this instance.
    /// </summary>
    /// <returns>
    ///     A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
    /// </returns>
    public override int GetHashCode() => HashCode.Combine(Title, ServerIcon, Colour, Pinned);

    /// <summary>
    ///     Implements the operator ==.
    /// </summary>
    public static bool operator ==(WaypointTemplateBase left, WaypointTemplateBase right) => Equals(left, right);

    /// <summary>
    ///     Implements the operator !=.
    /// </summary>
    public static bool operator !=(WaypointTemplateBase left, WaypointTemplateBase right) => !Equals(left, right);

    /// <summary>
    ///     Creates a new object that is a copy of the current instance.
    /// </summary>
    /// <returns>
    ///     A new object that is a copy of this instance.
    /// </returns>
    public object Clone()
    {
        return MemberwiseClone();
    }

    /// <summary>
    ///     Creates a new object that is a copy of the current instance.
    /// </summary>
    /// <returns>
    ///     A new object that is a copy of this instance.
    /// </returns>
    public virtual T Clone<T>() where T : WaypointTemplateBase
    {
        return (T)MemberwiseClone();
    }
}