using DelenDotNetChallenge2024;
using DelenDotNetChallenge2024.AssignmentModels;

// Parse the assignment file - There are multiple assignments (a, b, c, d, e)
var assignment = Assignment.ParseFromFile("a.in");
var solution = new List<SolutionLine>();

// Your solution here!! (add `SolutionLine`s in solutionLines)

// Test your solution directly
CalculateScore.RunLocally(assignment, solution);
