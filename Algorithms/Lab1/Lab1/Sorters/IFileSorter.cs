namespace Lab1.Sorters;

public interface IFileSorter
{
    void Sort(string fileName, out string sortedFileName, int helpFilesCount = 3);
}