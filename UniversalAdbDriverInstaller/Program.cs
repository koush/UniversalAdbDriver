using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Runtime;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using Org.BouncyCastle.Crypto.Prng;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.X509;
using Org.BouncyCastle.Utilities;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Asn1.Pkcs;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.X509.Extension;

namespace UniveralAdbDriverInstaller {
    class Program {
        public static X509Certificate2 GenerateCACertificate(string subjectName, int keyStrength = 2048) {
            // Generating Random Numbers
            var randomGenerator = new CryptoApiRandomGenerator();
            var random = new SecureRandom(randomGenerator);

            // The Certificate Generator
            var certificateGenerator = new X509V3CertificateGenerator();

            // Serial Number
            var serialNumber = BigIntegers.CreateRandomInRange(BigInteger.One, BigInteger.ValueOf(Int64.MaxValue), random);
            certificateGenerator.SetSerialNumber(serialNumber);

            // Signature Algorithm
            const string signatureAlgorithm = "SHA1WithRSA";
            certificateGenerator.SetSignatureAlgorithm(signatureAlgorithm);

            // Subject Public Key
            AsymmetricCipherKeyPair subjectKeyPair;
            var keyGenerationParameters = new KeyGenerationParameters(random, keyStrength);
            var keyPairGenerator = new RsaKeyPairGenerator();
            keyPairGenerator.Init(keyGenerationParameters);
            subjectKeyPair = keyPairGenerator.GenerateKeyPair();

            // Issuer and Subject Name
            var subjectDN = new X509Name(subjectName);
            var issuerDN = subjectDN;
            certificateGenerator.SetIssuerDN(issuerDN);
            certificateGenerator.SetSubjectDN(subjectDN);
//            certificateGenerator.AddExtension(X509Extensions.BasicConstraints, true, new BasicConstraints(false));
            certificateGenerator.AddExtension(X509Extensions.AuthorityKeyIdentifier, true, new AuthorityKeyIdentifier(SubjectPublicKeyInfoFactory.CreateSubjectPublicKeyInfo(subjectKeyPair.Public), new GeneralNames(new GeneralName(issuerDN)), serialNumber));

            // Valid For
            var notBefore = DateTime.UtcNow.Date;
            var notAfter = notBefore.AddYears(2);

            certificateGenerator.SetNotBefore(notBefore);
            certificateGenerator.SetNotAfter(notAfter);

            certificateGenerator.SetPublicKey(subjectKeyPair.Public);

            // Generating the Certificate
            var issuerKeyPair = subjectKeyPair;

            // selfsign certificate
            var certificate = certificateGenerator.Generate(issuerKeyPair.Private, random);
            var x509 = new System.Security.Cryptography.X509Certificates.X509Certificate2(certificate.GetEncoded());

            // Add CA certificate to Root store
            X509Store store = new X509Store(StoreName.Root, StoreLocation.LocalMachine);
            store.Open(OpenFlags.ReadWrite);
            store.Add(x509);
            store.Close();

            store = new X509Store(StoreName.TrustedPublisher, StoreLocation.LocalMachine);
            store.Open(OpenFlags.ReadWrite);
            store.Add(x509);
            store.Close();

            RsaPrivateCrtKeyParameters rsaparams = (RsaPrivateCrtKeyParameters)issuerKeyPair.Private;
            x509.PrivateKey = DotNetUtilities.ToRSA(rsaparams);
            store = new X509Store("PrivateCertStore", StoreLocation.CurrentUser);
            store.Open(OpenFlags.ReadWrite);
            store.Add(x509);
            store.Close();

            return x509;
        }

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

        static private void clearCerts(String storeName, StoreLocation location, params String[] subjectNames) {
            X509Store store = new X509Store(storeName, location);
            clearCerts(store, subjectNames);
        }

        static private void clearCerts(StoreName storeName, StoreLocation location, params String[] subjectNames) {
            X509Store store = new X509Store(storeName, location);
            clearCerts(store, subjectNames);
        }

        static private void clearCerts(X509Store store, params String[] subjectNames) {
            try {
                store.Open(OpenFlags.ReadWrite);
                foreach (var cert in store.Certificates) {
                    foreach (var sn in subjectNames) {
                        if (cert.Subject == sn)
                            store.Remove(cert);
                    }
                }
                store.Close();
            }
            catch (Exception) {
            }
        }

        static void Main(string[] args) {
            clearCerts(StoreName.Root, StoreLocation.LocalMachine, "CN=ClockworkMod", "CN=UniversalADB");
            clearCerts(StoreName.TrustedPublisher, StoreLocation.LocalMachine, "CN=ClockworkMod", "CN=UniversalADB");
            clearCerts("PrivateCertStore", StoreLocation.CurrentUser, "CN=UniversalADB");

            // sign the stuff
            ProcessStartInfo psi;
            Process p;
            var cerPath = Path.Combine(GetExecutablePath(), "UniversalADB.cer");
#if false
            var x509 = GenerateCACertificate("CN=UniversalADB");
#else
            //File.WriteAllBytes(x509.Export(X509ContentType.Pfx);

            psi = new ProcessStartInfo(Path.Combine(GetExecutablePath(), "makecert.exe"), "-r -pe -ss PrivateCertStore -n CN=UniversalADB \"" + cerPath + "\"");
            psi.WorkingDirectory = GetExecutablePath();
            psi.UseShellExecute = false;
            psi.CreateNoWindow = true;
            p = Process.Start(psi);
            p.WaitForExit();
            if (p.ExitCode != 0)
                throw new Exception("failure to create cert");

            X509Certificate2 x509 = new X509Certificate2(cerPath);
            // Add CA certificate to Root store
            X509Store store = new X509Store(StoreName.Root, StoreLocation.LocalMachine);
            store.Open(OpenFlags.ReadWrite);
            store.Add(x509);
            store.Close();

            store = new X509Store(StoreName.TrustedPublisher, StoreLocation.LocalMachine);
            store.Open(OpenFlags.ReadWrite);
            store.Add(x509);
            store.Close();

#endif
            
            psi = new ProcessStartInfo(Path.Combine(GetExecutablePath(), "signtool.exe"), "sign /v /s PrivateCertStore /n UniversalADB /t http://timestamp.verisign.com/scripts/timstamp.dll usb_driver\\androidwinusb86.cat");
            psi.WorkingDirectory = GetExecutablePath();
            psi.UseShellExecute = false;
            psi.CreateNoWindow = true;
            p = Process.Start(psi);
            p.WaitForExit();
            //if (p.ExitCode != 0)
            //    throw new Exception("failure to sign 86");

            psi = new ProcessStartInfo(Path.Combine(GetExecutablePath(), "signtool.exe"), "sign /v /s PrivateCertStore /n UniversalADB /t http://timestamp.verisign.com/scripts/timstamp.dll usb_driver\\androidwinusba64.cat");
            psi.WorkingDirectory = GetExecutablePath();
            psi.UseShellExecute = false;
            psi.CreateNoWindow = true;
            p = Process.Start(psi);
            p.WaitForExit();
            //if (p.ExitCode != 0)
            //    throw new Exception("failure to sign 64");

            // nuke the key from orbit, it's the only way to be sure
            clearCerts("PrivateCertStore", StoreLocation.LocalMachine, "CN=UniversalADB");

            // install the .inf
            SetupCopyOEMInf(Path.Combine(GetExecutablePath(), "usb_driver\\android_winusb.inf"), Path.Combine(GetExecutablePath(), "usb_driver"), (uint)OemSourcEMediaType.SPOST_PATH, 0, IntPtr.Zero, 0, IntPtr.Zero, IntPtr.Zero);
        }
    }
}