using RecServ.Global_Classes;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Text;

namespace RecServ.Modules
{
    internal class Screenshots
    {

        public void Start()
        {
            PathManager.Instance.CheckFolder(screenshotsFolder);
            InitializeScreenshot();
        }

        public void Stop()
        {
            screenshotTimer?.Dispose();
        }


#region > Constants and Other Initializers

        private static int intervalInMinutes = 1;
        private static string screenshotsFolder = PathManager.Instance.ScreenshotsCurrentDateFolder;
        private static System.Windows.Forms.Timer screenshotTimer;
        

#endregion

#region > Other Supportive Methods:

        public async static void InitializeScreenshot()
        {
            screenshotTimer = new System.Windows.Forms.Timer();

            // convert minutes to milliseconds
            screenshotTimer.Interval = intervalInMinutes * 20 * 100; // 1 Minute
            await Task.Delay(1000);
            screenshotTimer.Tick += new EventHandler(TakeScreenshot);
            screenshotTimer.Start();
        }

        private static string GetActiveWindowTitle()
        {
            IntPtr hWnd = GetForegroundWindow();

            if (hWnd == IntPtr.Zero)
            {
                return "Unknown App";
            }

            StringBuilder title = new StringBuilder(256);
            int length = GetWindowText(hWnd, title, title.Capacity);

            if (length > 0)
            {
                return title.ToString();
            }

            return "Unknown";
        }

#endregion

#region > Main Methods:

        private static void TakeScreenshot(object sender, EventArgs e)
        {
            try
            {
                // Get the dimensions of the screen
                Rectangle screenRect = Screen.PrimaryScreen.Bounds;

                // Create a Bitmap
                Bitmap bmp = new Bitmap(screenRect.Width, screenRect.Height, PixelFormat.Format32bppArgb);

                // Create a graphics object from the bitmap
                Graphics gfxBmp = Graphics.FromImage(bmp);

                // Take a screenshot of the screen
                gfxBmp.CopyFromScreen(screenRect.Left, screenRect.Top, 0, 0, screenRect.Size);

                string fileName = $"{screenshotsFolder}{GetActiveWindowTitle()} - {DateTime.Now:hh-mm tt}.jpg";

                // Save the screenshot as a .jpg file
                bmp.Save(fileName, ImageFormat.Jpeg);

                // Release resources
                gfxBmp.Dispose();
                bmp.Dispose();
            }
            catch (Exception ex)
            {
                //MessageBox.Show("Error! " + ex.Message);
            }
        }

#endregion

#region > Required DLL Files:

        [DllImport("user32.dll")]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);

        [DllImport("user32.dll")]
        static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        static extern bool GetWindowRect(IntPtr hWnd, out Rectangle rect);

#endregion

    }
}
