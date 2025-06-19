using Vintagestory.API.Common.Entities;

namespace ApacheTech.VintageMods.CampaignCartographer.Features.WaypointManager.WaypointTemplates;

/// <summary>
///     Represents a waypoint with a set position that can be added to the map within the game.
/// </summary>
[JsonObject]
[ProtoContract]
public class ImportedWaypointTemplate() : PositionedWaypointTemplate()
{
    /// <summary>
    ///     Initialises a new instance of the <see cref="ImportedWaypointTemplate"/> class by copying properties from an existing
    ///     <see cref="PositionedWaypointTemplate"/> and assigning a spawn position.
    /// </summary>
    /// <param name="template">The existing positioned waypoint template to copy properties from.</param>
    /// <param name="spawnPosition">The spawn position to assign.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="template"/> is null.</exception>
    public ImportedWaypointTemplate(PositionedWaypointTemplate template, EntityPos spawnPosition) : this()
    {
        if (template is null)
        {
            throw new ArgumentNullException(nameof(template));
        }

        // Copy properties from the base template.
        Id = template.Id;
        Title = template.Title;
        ServerIcon = template.ServerIcon;
        DisplayedIcon = template.DisplayedIcon;
        Colour = template.Colour;
        Pinned = template.Pinned;
        Position = template.Position;

        // Set the additional property.
        SpawnPosition = spawnPosition;
    }

    /// <summary>
    ///     The world spawn position, when the waypoint was exported.
    /// </summary>
    [JsonRequired]
    [ProtoMember(8)]
    public EntityPos SpawnPosition { get; set; } = default!;
}