using FluentFTP;

public class ProxyFTPClient
{
    private string _ftpHost = "127.0.0.1";          // FileZilla Server
    private string _username = "user";               
    private string _password = "1234";              
    private string _basePath = "/";            // FileZilla virtual mount root
    string localPath = @"C:\Temp\";     // Where files will be saved

    public async Task<List<string>> DownloadPqdFilesWithEdgeCaseAsync(DateTime afterDate, string localDownloadPath)
    {
        var downloadedFiles = new List<string>();
        var today = DateTime.Today;
        
        using (var client = new AsyncFtpClient(_ftpHost, _username, _password))
        {
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
                    // Check if any files in PQDIF folder were modified after afterDateTime
                    var pqdifFiles = await client.GetListing(pqdifPath, FtpListOption.Recursive);
                    var modifiedFiles = pqdifFiles
                        .Where(f => f.Type == FtpObjectType.File && f.Modified >= afterDateTime)
                        .ToList();
                    
                    if (modifiedFiles.Any())
                    {
                        // Download entire folder if any file was modified
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