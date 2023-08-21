using AForge.Video.DirectShow;
using AForge.Video;
using RecServ.Global_Classes;

namespace RecServ.Modules
{
    internal class WebCam
    {
        private System.Windows.Forms.Timer timer;
        private VideoCaptureDevice videoDevice;
        private string directory;
        private bool isRunning;

        public WebCam()
        {
            directory = PathManager.Instance.WebCamCurrentDateFolder;
            timer = new System.Windows.Forms.Timer();
            timer.Tick += Timer_Tick;
            timer.Interval = 10000; // Set the interval to 1 minute (60000 milliseconds)
        }

        public void Start()
        {
            if (!isRunning)
            {
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                FilterInfoCollection videoDevices = new FilterInfoCollection(AForge.Video.DirectShow.FilterCategory.VideoInputDevice);
                videoDevice = new VideoCaptureDevice(videoDevices[0].MonikerString);
                videoDevice.VideoResolution = videoDevice.VideoCapabilities[0];

                videoDevice.NewFrame += new NewFrameEventHandler(video_NewFrame);
                videoDevice.Start();
                System.Threading.Thread.Sleep(2000);
                videoDevice.SignalToStop();

                isRunning = true;
                timer.Start();
            }
        }

        public void Stop()
        {
            if (isRunning)
            {
                timer.Stop();
                videoDevice.SignalToStop();
                videoDevice.WaitForStop();
                videoDevice = null;
                isRunning = false;
            }
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            videoDevice.Start();
            System.Threading.Thread.Sleep(2000);
            videoDevice.SignalToStop();
        }

        private void video_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            string filename = "Img " + DateTime.Now.ToString("hh-mm tt") + ".jpg";
            string path = Path.Combine(directory, filename);

            int suffix = 1;
            string originalPath = path;
            while (File.Exists(path))
            {
                string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(originalPath);
                string extension = Path.GetExtension(originalPath);
                path = Path.Combine(directory, fileNameWithoutExtension + " (" + suffix + ")" + extension);
                suffix++;
            }

            eventArgs.Frame.Save(path);

            ((VideoCaptureDevice)sender).SignalToStop();
        }
    }
}
