using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace Ribo_Downloader
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            bool result;
            var mutex = new System.Threading.Mutex(true, "{7803901c-1e28-40d0-9f2e-884a3ab0e477}", out result);

            if (!result)
            {
                return;
            }
            if (!System.IO.Directory.Exists(VAR.LocalData))
            {
                System.IO.Directory.CreateDirectory(VAR.LocalData);
            }
            string pts = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\";
            if (!System.IO.File.Exists(VAR.LocalData+"save_path.txt"))
            {
                System.IO.TextWriter tw = new System.IO.StreamWriter(VAR.LocalData + "save_path.txt", true);
                tw.Write(pts);
                tw.Close();
            }
            else
            {
                pts = System.IO.File.ReadAllText(VAR.LocalData + "save_path.txt");
                pts = pts.Split(new string[] { "\r\n" }, StringSplitOptions.None)[0];
                VAR.SavePath = pts;
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Main());
            GC.KeepAlive(mutex);
        }
    }
}
