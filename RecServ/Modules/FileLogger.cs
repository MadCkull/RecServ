using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecServ.Modules
{
    internal class FileLogger
    {
        static FileSystemWatcher fileSystemWatcher;
        static string logFilePath = @"C:\AllLogs.txt";

        public void Start()
        {
            string directoryPath = @"D:\";

            if (!Directory.Exists(directoryPath))
            {
                MessageBox.Show("Invalid directory path!");
                return;
            }

            InitializeFileSystemWatcher(directoryPath);
        }

        public void Stop()
        {
            fileSystemWatcher.EnableRaisingEvents = false;
            fileSystemWatcher.Dispose();
        }



        static void InitializeFileSystemWatcher(string path)
        {
            fileSystemWatcher = new FileSystemWatcher();
            fileSystemWatcher.Path = path;

            fileSystemWatcher.NotifyFilter = NotifyFilters.DirectoryName |
                                             NotifyFilters.FileName |
                                             NotifyFilters.LastWrite |
                                             NotifyFilters.Size |
                                             NotifyFilters.CreationTime;

            fileSystemWatcher.IncludeSubdirectories = true;

            fileSystemWatcher.Created += FileSystemEventHandler;
            fileSystemWatcher.Deleted += FileSystemEventHandler;
            fileSystemWatcher.Renamed += RenamedEventHandler;
            fileSystemWatcher.Changed += FileSystemEventHandler;

            fileSystemWatcher.EnableRaisingEvents = true;
        }

        static void FileSystemEventHandler(object sender, FileSystemEventArgs e)
        {
            string eventType;
            switch (e.ChangeType)
            {
                case WatcherChangeTypes.Created:
                    eventType = "CREATED";
                    break;
                case WatcherChangeTypes.Deleted:
                    eventType = "DELETED";
                    break;
                case WatcherChangeTypes.Changed:
                    eventType = "MODIFIED";
                    break;
                default:
                    return;
            }

            string logEntry = $"[{DateTime.Now}] [{eventType}] {e.FullPath}";
            WriteToLog(logEntry);
        }

        static void RenamedEventHandler(object sender, RenamedEventArgs e)
        {
            string logEntry = $"[{DateTime.Now}] [RENAMED] {e.OldFullPath} > {e.FullPath}";
            WriteToLog(logEntry);
        }

        static void WriteToLog(string logEntry)
        {
            using (StreamWriter sw = File.AppendText(logFilePath))
            {
                sw.WriteLine(logEntry);
            }
        }
    }

}
