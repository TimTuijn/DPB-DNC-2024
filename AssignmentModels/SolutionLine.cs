namespace DelenDotNetChallenge2024.AssignmentModels;

public class SolutionLine
{
    public int Year { get; set; }
    public OfficeStatus Status { get; set; }
    public int CoordX { get; set; }
    public int CoordY { get; set; }

    public enum OfficeStatus
    {
        Construction,
        Destruction
    }

    public SolutionLine(int year, OfficeStatus status, int coordX, int coordY)
    {
        Year = year;
        Status = status;
        CoordX = coordX;
        CoordY = coordY;
    }
}