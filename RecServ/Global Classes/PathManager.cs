using System;
using System.IO;

namespace RecServ.Global_Classes;

public class PathManager
{
    private string currentDate;


    public string CurrentDate
    {
        get => currentDate;
        set => currentDate = value;
    }

    public string HomeDirectory { get; private set; }

    public string Username { get; private set; }

    public string Screenshots { get; private set; }

    public string ScreenshotsCurrentDateFolder { get; private set; }

    public string WebCamImages { get; private set; }

    public string WebCamCurrentDateFolder { get; private set; }

    public string Database { get; private set; }

    public string Other { get; private set; }

    public string TemporaryFiles { get; private set; }

    public string FetchedDatabaseFiles { get; private set; }


    public string LogsFilePath { get; private set; }



    private static PathManager? instance;

    private PathManager()
    {
        // Set Variable's Values Here
        currentDate = $"{DateTime.Now.Day}-{DateTime.Now.Month}-{DateTime.Now.Year}";

        LogsFilePath = @"D:\Other\RecServLogs.txt";



        HomeDirectory = @"D:\FYP\";

        Username = Path.Combine(HomeDirectory, Environment.UserName);

        Screenshots = Path.Combine(Username, "Screenshots");
            ScreenshotsCurrentDateFolder = Path.Combine(Screenshots, CurrentDate + @"\");

        WebCamImages = Path.Combine(Username, "WebCam Images");
            WebCamCurrentDateFolder = Path.Combine(WebCamImages, CurrentDate + @"\");

        Database = Path.Combine(Username, "Database");

        Other = Path.Combine(Username, "Other");

        TemporaryFiles = Path.Combine(Other, "Temporary Files");

        FetchedDatabaseFiles = Path.Combine(TemporaryFiles, "Fetched Database Files");
    }

    public static PathManager Instance => instance ??= new PathManager();



    public void CheckFolder(string folder)
    {
        if (!Directory.Exists(folder))
        {
            Directory.CreateDirectory(folder);
        }
    }
}
