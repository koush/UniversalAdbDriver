using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Runtime;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Windows.Forms;
using Newtonsoft.Json.Linq;

namespace AdbNativeMessaging {
    class Program {
        static String GetExecutablePath() {
            return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        }
        
        private static void adb(String argument, String message) {
            ProcessStartInfo psi;
            Process p;
            psi = new ProcessStartInfo(Path.Combine(GetExecutablePath(), "adb.exe"), argument);
            psi.WorkingDirectory = GetExecutablePath();
            psi.UseShellExecute = false;
            psi.CreateNoWindow = true;
            p = Process.Start(psi);
            p.WaitForExit();

            if (message != null)
                MessageBox.Show(message);
        }

        static void Main(string[] args) {
            if (args.Length == 1 && args[0] == "start-server") {
                adb("start-server", "ADB Server started.");
                return;
            }
            else if (args.Length == 1 && args[0] == "kill-server") {
                adb("kill-server", "ADB Server killed.");
                return;
            }

            byte[] len = new byte[4];
            var stdin = new BinaryReader(Console.OpenStandardInput());
            while (4 == stdin.Read(len, 0, 4)) {
                int l = BitConverter.ToInt32(len, 0);
                byte[] buf = new byte[l];
                stdin.Read(buf, 0, l);
                var str = System.Text.Encoding.UTF8.GetString(buf);
                var j = JObject.Parse(str);
                var cmd = (String)j["command"];
                adb(cmd, null);
            }
        }
    }
}
