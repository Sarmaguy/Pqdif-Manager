/// <summary>
/// Defines a contract for visiting files and directories asynchronously.
/// Implementations should provide logic for processing individual files and directory trees.
/// </summary>
public interface IFileVisitor
{
    /// <summary>
    /// Visits a single file asynchronously.
    /// </summary>
    /// <param name="filePath">The path to the file to visit.</param>
    Task VisitFileAsync(string filePath);
    
    /// <summary>
    /// Visits all files in a directory and its subdirectories asynchronously.
    /// </summary>
    /// <param name="directoryPath">The path to the directory to visit.</param>
    Task VisitDirectoryAsync(string directoryPath);
}
