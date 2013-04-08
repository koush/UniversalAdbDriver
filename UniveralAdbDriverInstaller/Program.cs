using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Runtime;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;

namespace UniveralAdbDriverInstaller {
    class Program {

        enum OemSourcEMediaType : uint {
            SPOST_PATH = 1
        };

        [DllImport("Setupapi.dll")]
        extern static int SetupCopyOEMInf(string SourceInfFileName,
             String OEMSourceMediaLocation,
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
            X509Certificate2 cert = new X509Certificate2(Path.Combine(GetExecutablePath(), "ClockworkMod.cer"));

            // add clockworkmod cert to root cert store
            X509Store store = new X509Store(StoreName.Root, StoreLocation.LocalMachine);
            store.Open(OpenFlags.ReadWrite);
            store.Add(cert);
            store.Close(); 

            // add clockworkmod cert to trusted publisher store
            store = new X509Store(StoreName.TrustedPublisher, StoreLocation.LocalMachine);
            store.Open(OpenFlags.ReadWrite);
            store.Add(cert);
            store.Close(); 

            // install the .inf
            SetupCopyOEMInf(Path.Combine(GetExecutablePath(), "usb_driver\\android_winusb.inf"), Path.Combine(GetExecutablePath(), "usb_driver"), (uint)OemSourcEMediaType.SPOST_PATH, 0, IntPtr.Zero, 0, IntPtr.Zero, IntPtr.Zero);
        }
    }
}