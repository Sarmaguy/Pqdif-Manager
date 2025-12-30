using FluentFTP;

/// <summary>
/// Handles downloading PQDIF files from an FTP server, supporting edge cases for modification time and folder structure.
/// Uses FluentFTP for asynchronous FTP operations.
/// </summary>
public class ProxyFTPClient
{
    private string _ftpHost = "127.0.0.1";          // FileZilla Server
    private string _username = "user";               
    private string _password = "1234";              
    private string _basePath = "/";            // FileZilla virtual mount root
    string localPath = @"C:\Temp\";     // Where files will be saved

    /// <summary>
    /// Downloads PQDIF files from the FTP server, handling edge cases for the first day and modification times.
    /// </summary>
    /// <param name="afterDate">Start date for file retrieval.</param>
    /// <param name="localDownloadPath">Local directory to save downloaded files.</param>
    /// <returns>List of downloaded file paths.</returns>
    public async Task<List<string>> DownloadPqdFilesWithEdgeCaseAsync(DateTime afterDate, string localDownloadPath)
    {
        var downloadedFiles = new List<string>();
        var today = DateTime.Today;
        
        using (var client = new AsyncFtpClient(_ftpHost, _username, _password))
        {
            //Force LIST mode if MLSD is problematic
            //client.Config.ListingParser = FtpParser.Windows;  
            // OR disable MLSD entirely:
            client.Config.DataConnectionType = FtpDataConnectionType.PASV;

            await client.Connect();
            
            // Handle first day with modification time check
            var firstDayFiles = await DownloadModifiedPqdifFoldersForDate(client, afterDate, afterDate, localDownloadPath);
            downloadedFiles.AddRange(firstDayFiles);
            
            // Process remaining days normally (download entire PQDIF folders)
            for (var date = afterDate.Date.AddDays(1); date <= today; date = date.AddDays(1))
            {
                string dayPath = $"{_basePath}/{date.Year}/Month_{date.Month:D2}/Day_{date.Day:D2}";
                
                try
                {
                    var dayItems = await client.GetListing(dayPath, FtpListOption.Auto);
                    var subfolders = dayItems.Where(item => item.Type == FtpObjectType.Directory).ToList();
                    
                    foreach (var subfolder in subfolders)
                    {
                        string pqdifPath = $"{subfolder.FullName}/PQDIF";
                        
                        if (await client.DirectoryExists(pqdifPath))
                        {
                            string localFolder = Path.Combine(localDownloadPath, 
                                $"{date:yyyy-MM-dd}", subfolder.Name, "PQDIF");
                            Directory.CreateDirectory(localFolder);
                            
                            await client.DownloadDirectory(localFolder, pqdifPath, 
                                FtpFolderSyncMode.Update, FtpLocalExists.Overwrite);
                            
                            var files = Directory.GetFiles(localFolder, "*.*", SearchOption.AllDirectories);
                            downloadedFiles.AddRange(files);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error processing {dayPath}: {ex.Message}");
                }
            }
            
            await client.Disconnect();
        }
        
        return downloadedFiles;
    }

    /// <summary>
    /// Downloads only PQDIF files modified after a specific date for a given day.
    /// </summary>
    /// <param name="client">The connected FTP client.</param>
    /// <param name="date">The day to check for modified files.</param>
    /// <param name="afterDateTime">The cutoff datetime for modification.</param>
    /// <param name="localDownloadPath">Local directory to save files.</param>
    /// <returns>List of downloaded file paths.</returns>
    private async Task<List<string>> DownloadModifiedPqdifFoldersForDate(AsyncFtpClient client, 
        DateTime date, DateTime afterDateTime, string localDownloadPath)
    {
        var downloadedFiles = new List<string>();
        string dayPath = $"{_basePath}/{date.Year}/Month_{date.Month:D2}/Day_{date.Day:D2}";
        
        try
        {
            var dayItems = await client.GetListing(dayPath, FtpListOption.Auto);
            var subfolders = dayItems.Where(item => item.Type == FtpObjectType.Directory).ToList();
            
            foreach (var subfolder in subfolders)
            {
                string pqdifPath = $"{subfolder.FullName}/PQDIF";
                
                if (await client.DirectoryExists(pqdifPath))
                {
                    var pqdifFiles = await client.GetListing(pqdifPath, FtpListOption.Recursive);
                    var modifiedFiles = new List<FtpListItem>();
                    
                    // Verify each file's modification time using MDTM
                    foreach (var file in pqdifFiles.Where(f => f.Type == FtpObjectType.File))
                    {
                        try
                        {
                            // Use MDTM command directly for accurate timestamp
                            var modTime = await client.GetModifiedTime(file.FullName);
                            if (modTime >= afterDateTime)
                            {
                                modifiedFiles.Add(file);
                            }
                        }
                        catch
                        {
                            // If MDTM fails, fall back to parsed Modified property
                            if (file.Modified >= afterDateTime)
                            {
                                modifiedFiles.Add(file);
                            }
                        }
                    }
                    
                    if (modifiedFiles.Any())
                    {
                        string localFolder = Path.Combine(localDownloadPath, 
                            $"{date:yyyy-MM-dd}", subfolder.Name, "PQDIF");
                        Directory.CreateDirectory(localFolder);
                        
                        await client.DownloadDirectory(localFolder, pqdifPath, 
                            FtpFolderSyncMode.Update, FtpLocalExists.Overwrite);
                        
                        var files = Directory.GetFiles(localFolder, "*.*", SearchOption.AllDirectories);
                        downloadedFiles.AddRange(files);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error checking modified files for {dayPath}: {ex.Message}");
        }
        
        return downloadedFiles;
    }
}