using ColoringABC;

var graph = Graph.GenerateGraph(200, 20);
var abc = new ABC(graph, 2, 28);
var result = abc.Solve(printIterations: true);
Console.WriteLine("==================Result==================");
Console.WriteLine(result.IsValid ? $"ChromaticNumber: {result.ChromaticNumber}" : "Solution is incorrect");