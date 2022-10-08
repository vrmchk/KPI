namespace Lab1.Sorters;

public class BinaryFileSorter : IFileSorter
{
    public void Sort(string fileName, out string sortedFileName, int helpFilesCount = 3)
    {
        if (helpFilesCount < 2)
            throw new ArgumentException(null, nameof(helpFilesCount));
        string[] bHelpFiles = Enumerable.Range(1, helpFilesCount).Select(i => $"B{i}.dat").ToArray();
        string[] cHelpFiles = Enumerable.Range(1, helpFilesCount).Select(i => $"C{i}.dat").ToArray();
        SplitFile(fileName, bHelpFiles);
        SortHelper(bHelpFiles, cHelpFiles, out sortedFileName);
    }

    public void SortParts(string fileName, string outputFileName, int size, int shareSize)
    {
        if (File.Exists(outputFileName))
        {
            File.Delete(outputFileName);
        }

        int[] array = new int[shareSize];
        using var reader = new BinaryReader(File.Open(fileName, FileMode.Open));
        using var writer = new BinaryWriter(File.Open(outputFileName, FileMode.OpenOrCreate));
        for (int i = 0; i < size / shareSize; i++)
        {
            for (int j = 0; j < shareSize; j++)
            {
                array[j] = reader.ReadInt32();
            }

            Array.Sort(array);
            for (int j = 0; j < shareSize; j++)
            {
                writer.Write(array[j]);
            }
        }
    }

    private void SortHelper(string[] bHelpFiles, string[] cHelpFiles, out string fileName)
    {
        var readers = bHelpFiles.Select(f => new BinaryReader(File.OpenRead(f))).ToList();
        readers.Where(r => r.EndOfStream()).ToList().ForEach(r =>
        {
            r.Dispose();
            readers.Remove(r);
        });

        if (readers.Count == 1)
        {
            fileName = ((FileStream) readers.First().BaseStream).Name;
            return;
        }

        var writers = cHelpFiles.Select(f => new BinaryWriter(File.Open(f, FileMode.Create))).ToList();
        var currentWriter = writers.First();
        var currentReader = readers.First();
        var nums = new List<int>();
        var nextNums = new List<int>();
        var readerAndPrevNum = readers.ToDictionary(r => r, _ => int.MinValue);

        while (readers.Count != 0)
        {
            while (currentReader.EndOfStream())
            {
                var readerToRemove = currentReader;
                currentReader = readers.NextAfter(currentReader);
                readers.Remove(readerToRemove);
                readerAndPrevNum.Remove(readerToRemove);
                readerToRemove.Dispose();
                if (readers.Count == 0)
                {
                    break;
                }
            }

            if (readers.Count == 0)
            {
                nums.Sort();
                foreach (int n in nums)
                {
                    currentWriter.Write(n);
                }

                currentWriter = writers.NextAfter(currentWriter);
                nextNums.Sort();
                foreach (int n in nextNums)
                {
                    currentWriter.Write(n);
                }

                break;
            }

            int num = currentReader.ReadInt32();
            if (num >= readerAndPrevNum[currentReader])
            {
                nums.Add(num);
                readerAndPrevNum[currentReader] = num;
            }
            else
            {
                nextNums.Add(num);
                readerAndPrevNum[currentReader] = num;
                currentReader = readers.NextAfter(currentReader);
            }

            if (nextNums.Count >= readers.Count)
            {
                nums.Sort();
                foreach (int n in nums)
                {
                    currentWriter.Write(n);
                }

                currentWriter = writers.NextAfter(currentWriter);
                nums.Clear();
                nums.AddRange(nextNums);
                nextNums.Clear();
            }
        }

        readers.ForEach(r => r.Dispose());
        writers.ForEach(w => w.Dispose());
        SortHelper(cHelpFiles, bHelpFiles, out fileName);
    }


    private void SplitFile(string fileName, string[] bHelpFiles)
    {
        var writers = bHelpFiles.Select(f => new BinaryWriter(File.Open(f, FileMode.Create))).ToList();
        var currentWriter = writers.First();
        using var reader = new BinaryReader(File.OpenRead(fileName));
        int previousNum = int.MinValue;

        while (!reader.EndOfStream())
        {
            int num = reader.ReadInt32();
            if (num >= previousNum)
            {
                currentWriter.Write(num);
            }
            else
            {
                currentWriter = writers.NextAfter(currentWriter);
                currentWriter.Write(num);
            }

            previousNum = num;
        }

        writers.ForEach(w => w.Dispose());
    }
}