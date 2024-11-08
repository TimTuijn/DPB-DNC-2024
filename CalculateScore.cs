using System.ComponentModel.DataAnnotations;
using DelenDotNetChallenge2024.AssignmentModels;

namespace DelenDotNetChallenge2024;

public static class CalculateScore
{
    public static void RunLocally(Assignment assignment, List<SolutionLine> solution)
    {
        double score = 0;
        try
        {
            score = ValidateAndScore(assignment, solution);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
        Console.WriteLine($"Your score is {score}!");
    }

    private static double ValidateAndScore(Assignment assignment, List<SolutionLine> solution)
    {
        Validate(assignment, solution);
        return ScoreSolution(assignment, solution);
    }

    private static void Validate(Assignment assignment, List<SolutionLine> solutionLines)
    {
        ValidateSolutionNotEmpty(solutionLines);
        ValidateMaxYear(assignment, solutionLines);
        ValidateMinYear(assignment, solutionLines);
        ValidateOfficeLocationsInGrid(assignment, solutionLines);
        ValidateOfficeLifeTime(assignment, solutionLines);
    }

    private static void ValidateOfficeLifeTime(Assignment assignment, List<SolutionLine> solutionLines)
    {
        var officesPreviousYear = new List<ValueTuple<int, int>>();
        foreach (var grouping in solutionLines.GroupBy(x => x.Year))
        {
            var year = grouping.Key;
            var lines = grouping.ToList();

            var officesToAdd = new List<ValueTuple<int, int>>();
            var officesToDestruct = new List<ValueTuple<int, int>>();

            foreach (var line in lines)
            {
                var office = new ValueTuple<int, int>(line.CoordX, line.CoordY);

                switch (line.Status)
                {
                    case SolutionLine.OfficeStatus.Construction:
                        if (officesPreviousYear.Any(o => HasOverlap(o, office)))
                        {
                            throw new ValidationException(
                                $"Validation failed: year {year}: office on coordinates {office.Item1}, {office.Item2} is constructed, but has overlap with another office.");
                        }

                        officesToAdd.Add(office);
                        break;
                    case SolutionLine.OfficeStatus.Destruction:
                        if (officesPreviousYear.All(o => o != office))
                        {
                            throw new ValidationException(
                                $"Validation failed: year {year}: office on coordinates {office.Item1}, {office.Item2} is destructed, but there is no office on this location.");
                        }

                        officesToDestruct.Add(office);
                        break;
                }
            }

            officesPreviousYear.AddRange(officesToAdd);
            if (officesPreviousYear.Count > assignment.MaximumYearlyOffices[year])
            {
                throw new ValidationException(
                    $"Validation failed: year {year}: {officesPreviousYear.Count} offices exceeds maximum yearly.");
            }

            // offices that are removed still count towards the validation of that year
            officesPreviousYear.RemoveAll(o => officesToDestruct.Contains(o));
        }

        return;

        bool HasOverlap(ValueTuple<int, int> office1, ValueTuple<int, int> office2)
        {
            return Math.Abs(office1.Item1 - office2.Item1) <= 1 && Math.Abs(office1.Item2 - office2.Item2) <= 1;
        }
    }


    private static void ValidateSolutionNotEmpty(List<SolutionLine> solutionLines)
    {
        if (solutionLines.Count == 0)
        {
            throw new ValidationException("Validation failed: No solution lines were provided.");
        }
    }

    private static void ValidateOfficeLocationsInGrid(Assignment assignment, List<SolutionLine> solutionLines)
    {
        foreach (var solutionLine in solutionLines)
        {
            if (solutionLine.CoordX >= assignment.OfficeMaxX || solutionLine.CoordX < 0 ||
                solutionLine.CoordY >= assignment.OfficeMaxY || solutionLine.CoordY < 0)
            {
                throw new ValidationException(
                    $"Validation failed: office location in year {solutionLine.Year} is out of range: {solutionLine.CoordX}, {solutionLine.CoordY}");
            }
        }
    }

    private static void ValidateMinYear(Assignment assignment, List<SolutionLine> solutionLines)
    {
        var minYear = solutionLines.Min(l => l.Year);
        if (minYear < 0)
        {
            throw new ValidationException($"Validation failed: year in solution ({minYear}) is not valid.");
        }
    }

    private static void ValidateMaxYear(Assignment assignment, List<SolutionLine> solutionLines)
    {
        var maxYear = solutionLines.Max(l => l.Year);
        if (maxYear >= assignment.YearsToSimulate)
        {
            throw new ValidationException(
                $"Validation failed: year in solution ({maxYear}) is higher than years to simulate ({assignment.YearsToSimulate}).");
        }
    }

    private static double ScoreSolution(Assignment assignment, List<SolutionLine> solutionLines)
    {
        var score = 0.0;

        AddActiveOfficesPerYear(assignment, solutionLines);

        var solutionLinesPerYear = solutionLines
            .GroupBy(x => x.Year)
            .ToDictionary(x => x.Key, x => x.ToList());

        for (var year = 0; year < assignment.YearsToSimulate; year++)
        {
            // If no offices to evaluate this year, continue to next year
            if (!solutionLinesPerYear.TryGetValue(year, out var officesForYear))
            {
                continue;
            }

            score += ScoreSolutionYear(assignment, officesForYear, year);
        }

        return score;
    }

    private static void AddActiveOfficesPerYear(Assignment assignment, List<SolutionLine> solutionLines)
    {
        foreach (var grouping in solutionLines.ToLookup(l => (l.CoordX, l.CoordY)))
        {
            var (coordX, coordY) = grouping.Key;
            var lastConstructionYear = -1;
            var lastStatus = SolutionLine.OfficeStatus.Destruction;
            foreach (var line in grouping.OrderBy(l => l.Year))
            {
                // We don't care about validation, validation happens earlier
                switch (line.Status)
                {
                    case SolutionLine.OfficeStatus.Construction:
                        lastConstructionYear = line.Year;
                        lastStatus = SolutionLine.OfficeStatus.Construction;
                        break;
                    case SolutionLine.OfficeStatus.Destruction:
                    {
                        GenerateSolutionLinesForRange(coordX, coordY, lastConstructionYear + 1, line.Year);
                        lastStatus = SolutionLine.OfficeStatus.Destruction;
                        break;
                    }
                }
            }

            if (lastStatus == SolutionLine.OfficeStatus.Construction)
            {
                GenerateSolutionLinesForRange(coordX, coordY, lastConstructionYear + 1, assignment.YearsToSimulate);
            }
        }

        return;

        void GenerateSolutionLinesForRange(int coordX, int coordY, int startYear, int yearToStop)
        {
            solutionLines.AddRange(Enumerable.Range(startYear, yearToStop - startYear)
                .Select(year => new SolutionLine(year, SolutionLine.OfficeStatus.Construction, coordX, coordY))
            );
        }
    }

    private static double ScoreSolutionYear(Assignment assignment, IEnumerable<SolutionLine> officesForYear, int year)
    {
        var offices = officesForYear.ToList();

        // All lines with 'construction' will generate revenue, destruction generates no revenue
        var officesGeneratingRevenue = offices.Where(o => o.Status != SolutionLine.OfficeStatus.Destruction);
        var revenue = 0.0;
        foreach (var office in officesGeneratingRevenue)
        {
            var x = office.CoordX;
            var y = office.CoordY;
            const double factor = 0.2;

            revenue += assignment.OfficeLocations[x][y].PotentialRevenue[year];
            revenue += factor * assignment.OfficeLocations.ElementAtOrDefault(x - 1)?.ElementAtOrDefault(y - 1)
                ?.PotentialRevenue[year] ?? 0;
            revenue += factor * assignment.OfficeLocations.ElementAtOrDefault(x - 1)?.ElementAtOrDefault(y)
                ?.PotentialRevenue[year] ?? 0;
            revenue += factor * assignment.OfficeLocations.ElementAtOrDefault(x - 1)?.ElementAtOrDefault(y + 1)
                ?.PotentialRevenue[year] ?? 0;
            revenue += factor * assignment.OfficeLocations.ElementAtOrDefault(x)?.ElementAtOrDefault(y - 1)
                ?.PotentialRevenue[year] ?? 0;
            revenue += factor * assignment.OfficeLocations.ElementAtOrDefault(x)?.ElementAtOrDefault(y + 1)
                ?.PotentialRevenue[year] ?? 0;
            revenue += factor * assignment.OfficeLocations.ElementAtOrDefault(x + 1)?.ElementAtOrDefault(y - 1)
                ?.PotentialRevenue[year] ?? 0;
            revenue += factor * assignment.OfficeLocations.ElementAtOrDefault(x + 1)?.ElementAtOrDefault(y)
                ?.PotentialRevenue[year] ?? 0;
            revenue += factor * assignment.OfficeLocations.ElementAtOrDefault(x + 1)?.ElementAtOrDefault(y + 1)
                ?.PotentialRevenue[year] ?? 0;
        }

        return revenue;
    }
}