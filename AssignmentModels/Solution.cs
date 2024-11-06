using System.Text.Json;

namespace DelenDotNetChallenge2024.AssignmentModels;

public class Solution
{
    public static void CreateSolutionFile(IEnumerable<SolutionLine> lines, string fileName)
    {
        var jsonString = JsonSerializer.Serialize(lines);
        File.WriteAllText(fileName, jsonString);
    }
}