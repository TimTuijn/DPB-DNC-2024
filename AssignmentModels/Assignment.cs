using System.Text.Json;

namespace DelenDotNetChallenge2024.AssignmentModels;

public class Assignment
{
    public int YearsToSimulate { get; set; }
    public int OfficeMaxX { get; set; }
    public int OfficeMaxY { get; set; }
    public List<int> MaximumYearlyOffices { get; set; } = [];
    public List<List<Office>> OfficeLocations { get; set; } = [];

    public static Assignment ParseFromFile(string fileName)
    {
        var jsonString = File.ReadAllText(fileName);
        var assignment = JsonSerializer.Deserialize<Assignment>(jsonString);

        return assignment ?? throw new Exception("Something went wrong parsing the assignment");
    }
}
