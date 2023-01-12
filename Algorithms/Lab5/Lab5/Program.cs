using System.Text;
using Lab5;

var problem  = new Problem();
var result = new StringBuilder();
var batchNumber = 4;
var probability = 0.3;
var algorithm = new GeneticAlgorithm(batchNumber, probability, problem);
for (int i = 0; i < 500; i++)
{
    for (int j = 0; j < 20; j++)
    {
        algorithm.Iterate();
    }

    var line = $"Iteration: {i * 20 + 20}, Cost: {algorithm.GetBestSolution().Cost}";
    result.AppendLine(line);
    Console.WriteLine(line);
}

File.WriteAllText("result.txt", result.ToString());