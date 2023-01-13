namespace Lab4;

class Solution
{
    public int[] ColorSet { get; set; }
    public int[][] Graph { get; set; }

    public Solution(int[][] graph, int[] solution)
    {
        Graph = graph;
        ColorSet = solution;
    }
    public double Fitness
    {
        get {
            int violations = 0;
            for (int i = 0; i < Graph.Length; i++)
            {
                for (int j = 0; j < Graph[i].Length; j++)
                {
                    if (ColorSet[i] == ColorSet[Graph[i][j]] && i != j)
                        violations++;
                }
            }
            int chromaticNumber = ChromaticNumber(Graph);
            int numDistinctColors = ColorSet.Distinct().Count();
            int totalColors = chromaticNumber * Graph.Length;
            return 1 - (double)(violations + numDistinctColors) / totalColors;
        }
    }
    private static int ChromaticNumber(int[][] graph)
    {
        int[] colors = new int[graph.Length];
        Array.Fill(colors, -1);
        colors[0] = 0;

        bool[] availableColors = new bool[graph.Length];
        for (int i = 0; i < graph.Length; i++)
        {
            Array.Fill(availableColors, true);
            for (int j = 0; j < graph[i].Length; j++)
            {
                int neighbor = graph[i][j];
                if (colors[neighbor] != -1)
                    availableColors[colors[neighbor]] = false;
            }

            int color = 0;
            for (int j = 0; j < availableColors.Length; j++)
            {
                if (availableColors[j])
                {
                    color = j;
                    break;
                }
            }
            colors[i] = color;
        }

        return colors.Max() + 1;
    }
}