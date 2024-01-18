class Program
{
    static Dictionary<string, object> nestedTree = new Dictionary<string, object>();

    static async Task Main()
    {
        string InputDataPath = "InputData.txt";
        string OutputDataPath = "OutputData.txt";

        await ReadFile(InputDataPath);
        var sortedTree = SortNestedTree(nestedTree);
        await WriteSortedTreeToFile(sortedTree, OutputDataPath);
    }

    static async Task ReadFile(string path)
    {
        using (StreamReader reader = new StreamReader(path))
        {
            string? line;
            while ((line = await reader.ReadLineAsync()) != null)
            {
                ProcessLine(line);
            }
        }
    }

    static void ProcessLine(string line)
    {
        string[] names = line.Split('\\');

        Dictionary<string, object> currentDictionary = nestedTree;

        for (int i = 0; i < names.Length; i++)
        {
            string name = names[i];
            
            if (!currentDictionary.ContainsKey(name))
            {
                UpdateTree(currentDictionary, name);
            }

            if (i < names.Length - 1)
            {
                currentDictionary = currentDictionary[name] as Dictionary<string, object> ?? new Dictionary<string, object>();
            }
        }
    }

    static void UpdateTree(Dictionary<string, object> currentDictionary, string name)
    {
        string[] parts = name.Split(' ');

        if (parts.Length == 1)
        {
            currentDictionary.Add(name, new Dictionary<string, object>());
        }
        else if (parts.Length == 2)
        {
            if (int.TryParse(parts[1], out int sizeFile))
            {
                currentDictionary.Add(parts[0], sizeFile);
            }
        }
    }

    static Dictionary<string, object> SortNestedTree(Dictionary<string, object> nestedTree)
    {
        var folders = nestedTree
            .Where(kv => kv.Value is Dictionary<string, object>)
            .OrderBy(kv => kv.Key, StringComparer.OrdinalIgnoreCase)
            .Select(kv => new KeyValuePair<string, object>(kv.Key, SortNestedTree((Dictionary<string, object>)kv.Value)));

        var files = nestedTree
            .Where(kv => kv.Value is int)
            .OrderByDescending(kv => (int)kv.Value)
            .ThenBy(kv => kv.Key, StringComparer.OrdinalIgnoreCase);

        return folders.Concat(files).ToDictionary(kv => kv.Key, kv => kv.Value);
    }

    static async Task WriteSortedTreeToFile(Dictionary<string, object> nestedTree, string path)
    {
        using (StreamWriter writer = new StreamWriter(path, false))
        {
            await WriteNestedTree(nestedTree, writer);
        }
    }

    static async Task WriteNestedTree(Dictionary<string, object> nestedTree, StreamWriter writer, string indent = "")
    {
        foreach (var kvp in nestedTree)
        {
            await writer.WriteLineAsync($"{indent}{kvp.Key}");

            if (kvp.Value is Dictionary<string, object> nestedDictionary)
            {
                await WriteNestedTree(nestedDictionary, writer, indent + "  ");
            }
        }
    }
}
