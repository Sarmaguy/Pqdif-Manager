public interface IFileVisitor
{
    Task VisitFileAsync(string filePath);
    
    // Visit all files in a directory and subdirectories
    Task VisitDirectoryAsync(string directoryPath);
}
