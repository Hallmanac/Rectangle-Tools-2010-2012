
/*(C) Copyright 2010 by Autodesk, Inc.
 * Original Author of the AddInInstaller class code is Philippe Leefsma
 * Only the namespace was changed by Brian Hall for use with this solution.*/

using System;
using System.Collections;
using System.ComponentModel;
using System.Configuration.Install;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace QubeItTools.General
{
    [RunInstaller(true)]
    public partial class AddInInstaller : Installer
    {
        [DllImport("kernel32.dll", SetLastError = true, CallingConvention = CallingConvention.Winapi)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool IsWow64Process([In] IntPtr hProcess,
            [Out] out bool wow64Process);

        public AddInInstaller()
        {
            InitializeComponent();
        }

        public override void Install(IDictionary stateSaver)
        {
            base.Install(stateSaver);

            bool is64 = false;
            IsWow64Process(Process.GetCurrentProcess().Handle, out is64);

            if(is64)
            {
                RegAsm64("/codebase");
            }
            else
            {
                RegistrationServices regsrv = new RegistrationServices();
                regsrv.RegisterAssembly(GetType().Assembly, AssemblyRegistrationFlags.SetCodeBase);
            }
        }

        public override void Uninstall(IDictionary savedState)
        {
            base.Uninstall(savedState);

            bool is64 = false;
            IsWow64Process(Process.GetCurrentProcess().Handle, out is64);

            if(is64)
            {
                RegAsm64("/u");
            }
            else
            {
                RegistrationServices regsrv = new RegistrationServices();
                regsrv.UnregisterAssembly(GetType().Assembly);
            }

            base.Uninstall(savedState);
        }

        private static void RegAsm64(string parameters)
        {
            //.Net Framework Path
            string fmwk_path = Path.GetFullPath(
                Path.Combine(RuntimeEnvironment.GetRuntimeDirectory(), "..\\.."));

            //RegAsm Path
            string regasm_path = string.Concat(fmwk_path,
                "\\Framework64\\",
                RuntimeEnvironment.GetSystemVersion(),
                "\\regasm.exe");

            if(!File.Exists(regasm_path))
            {
                MessageBox.Show("Failed to find RegAsm",
                    "Installer Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return;
            }

            string addin_path = System.Reflection.Assembly.GetExecutingAssembly().Location;

            System.Diagnostics.Process process = new System.Diagnostics.Process
            {
                StartInfo =
                {
                    CreateNoWindow = true,
                    ErrorDialog = false,
                    UseShellExecute = false,
                    FileName = regasm_path,
                    Arguments = string.Format("\"{0}\" {1}", addin_path, parameters)
                }
            };

            using(process)
            {
                process.Start();
                process.WaitForExit();
            }
        }
    }
}
