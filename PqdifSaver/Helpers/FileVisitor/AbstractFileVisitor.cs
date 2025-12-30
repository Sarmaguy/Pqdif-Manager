/// <summary>
/// Base implementation of IFileVisitor supporting rule-based file and directory traversal.
/// Allows adding rules for file matching and processing actions.
/// </summary>
public class AbstractFileVisitor : IFileVisitor
{
    private readonly List<(Func<string, bool> Match, Func<string, Task> Action)> _rules = new();

    /// <summary>
    /// Adds a rule for matching files and specifying an action to perform.
    /// </summary>
    /// <param name="match">Predicate to determine if the rule applies to a file path.</param>
    /// <param name="action">Action to perform if the rule matches.</param>
    /// <returns>The current AbstractFileVisitor instance for chaining.</returns>
    public AbstractFileVisitor AddRule(Func<string, bool> match, Func<string, Task> action)
    {
        _rules.Add((match, action));
        return this;
    }

    /// <summary>
    /// Visits a single file and applies all matching rules.
    /// </summary>
    /// <param name="filePath">The path to the file to visit.</param>
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

    /// <summary>
    /// Recursively visits all files in a directory and its subdirectories, applying rules to each file.
    /// </summary>
    /// <param name="directoryPath">The path to the directory to visit.</param>
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
