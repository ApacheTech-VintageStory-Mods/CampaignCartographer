using System.Reflection;
using System.Reflection.Emit;
using System.Text;

namespace ApacheTech.VintageMods.CampaignCartographer.Features.WaypointManager.Patches;

public static class CodeInstructionExtensions
{
    /// <summary>
    ///     Converts a collection of <see cref="CodeInstruction"/> objects into a readable string representation with each
    /// entry prefixed by its index and returns the result in a <see cref="StringBuilder"/>.
    /// </summary>
    /// <param name="instructions">The collection of <see cref="CodeInstruction"/> objects.</param>
    /// <returns>A <see cref="StringBuilder"/> containing the formatted instructions.</returns>
    public static StringBuilder ToStringBuilder(this IEnumerable<CodeInstruction> instructions)
    {
        var builder = new StringBuilder();
        builder.AppendLine();
        var index = 0;

        foreach (var instruction in instructions)
        {
            var result = instruction.opcode.ToString();

            if (instruction.operand != null)
            {
                result += " " + (instruction.operand switch
                {
                    MethodBase method => $"{method.DeclaringType?.FullName}::{method.Name}",
                    FieldInfo field => $"{field.DeclaringType?.FullName}::{field.Name}",
                    LocalBuilder local => $"Local_{local.LocalIndex} ({local.LocalType.Name})",
                    Label label => $"Label_{label.GetHashCode()}",
                    string str => $"\"{str}\"",
                    _ => instruction.operand.ToString()
                });
            }

            if (instruction.labels?.Count > 0)
            {
                var labels = string.Join(", ", instruction.labels.Select(l => $"Label_{l.GetHashCode()}"));
                result = $"[{labels}] {result}";
            }

            if (instruction.blocks?.Count > 0)
            {
                var blocks = string.Join(", ", instruction.blocks.Select(b => b.ToString()));
                result += $" // Blocks: {blocks}";
            }

            builder.AppendLine($"{index}: {result}");
            index++;
        }

        return builder;
    }
}