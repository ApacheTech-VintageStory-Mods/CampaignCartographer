using System.IO;

namespace ApacheTech.VintageMods.CampaignCartographer.Domain.ChatCommands.Extensions;

/// <summary>
///     Extension methods that add functionality to <see cref="FileInfo"/> instances.
/// </summary>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public static class FileInfoExtensions
{
    /// <summary>
    ///     Renames a file.
    /// </summary>
    /// <param name="file">The file to rename.</param>
    /// <param name="newName">The new name.</param>
    public static void Rename(this FileInfo file, string newName)
    {
        var newPath = Path.Combine(file.DirectoryName!, newName);
        file.MoveTo(newPath);
    }
}