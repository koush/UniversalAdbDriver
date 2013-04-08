using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Runtime;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace UniveralAdbDriverInstaller {
    class Program {

        [DllImport("Setupapi.dll")]
        extern static int SetupCopyOEMInf(string SourceInfFileName,
             IntPtr OEMSourceMediaLocation,
             uint OEMSourceMediaType,
             uint CopyStyle,
             IntPtr DestinationInfFileName,
             uint DestinationInfFileNameSize,
             IntPtr RequiredSize,
             IntPtr DestinationInfFileNameComponent
        );

        static String GetExecutablePath() {
            return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        }

        static void Main(string[] args) {
            // add clockworkmod cert to root cert store
            Process process = Process.Start("certutil.exe", String.Format("-addstore -f Root {0}", Path.Combine(GetExecutablePath(), "ClockworkMod.cer")));
            process.WaitForExit();

            // add clockworkmod cert to trusted publishers
            process = Process.Start("certutil.exe", String.Format("-addstore -f TrustedPublisher {0}", Path.Combine(GetExecutablePath(), "ClockworkMod.cer")));
            process.WaitForExit();

            // install the .inf
            SetupCopyOEMInf(Path.Combine(GetExecutablePath(), "usb_driver\\android_winusb.inf"), IntPtr.Zero, 0, 0, IntPtr.Zero, 0, IntPtr.Zero, IntPtr.Zero);
        }
    }
}