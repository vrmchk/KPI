namespace ColoringABC.Hive;

class OnlookerBee
{
    public void SetVertexColor(Vertex vertex, List<int> usedColors, Queue<int> availableColors)
    {
        var usedColorIndex = 0;
        while (!vertex.IsValid || vertex.ColorValue == -1)
        {
            if (usedColorIndex == usedColors.Count - 1 || usedColors.Count == 0)
            {
                var color = availableColors.Dequeue();
                vertex.ColorValue = color;
                usedColors.Add(color);
                return;
            }

            vertex.ColorValue = usedColors[usedColorIndex++];
        }
    }
}