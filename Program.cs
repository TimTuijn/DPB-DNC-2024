using DelenDotNetChallenge2024.AssignmentModels;

// Parse the assignment file
var assignment = Assignment.ParseFromFile("a.in");
var solution = new List<SolutionLine>();

// Your solution here!! (add `SolutionLine`s in solutionLines)

Solution.CreateSolutionFile(solution, "a.out");
