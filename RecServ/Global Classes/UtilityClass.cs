using RecServ.Global_Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecServ
{
    public static class Logger
    {
        
        public static void WriteLog(string message)
        {
            var currentTime = DateTime.Now.ToString("[dd-MM-yyyy hh:mm:ss tt]  ");

            try { File.AppendAllText(PathManager.Instance.LogsFilePath, $"{currentTime}{message}\n"); }

            catch { }
            
        }
    }
}
