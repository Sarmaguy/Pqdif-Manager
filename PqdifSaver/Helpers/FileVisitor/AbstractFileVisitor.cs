public class AbstractFileVisitor : IFileVisitor
{
    private readonly List<(Func<string, bool> Match, Func<string, Task> Action)> _rules = new();

    public AbstractFileVisitor AddRule(Func<string, bool> match, Func<string, Task> action)
    {
        _rules.Add((match, action));
        return this;
    }

    public async Task VisitFileAsync(string filePath)
    {
        foreach (var rule in _rules)
        {
            if (rule.Match(filePath))
            {
                await rule.Action(filePath);
            }
        }
    }

    public async Task VisitDirectoryAsync(string directoryPath)
    {
        // Process files in current directory
        foreach (var file in Directory.GetFiles(directoryPath))
        {
            await VisitFileAsync(file);
        }

        // Recurse into subdirectories
        foreach (var subDir in Directory.GetDirectories(directoryPath))
        {
            await VisitDirectoryAsync(subDir);
        }
    }
}
