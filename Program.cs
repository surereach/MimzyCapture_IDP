using System.DirectoryServices.ActiveDirectory;
using System.Runtime.InteropServices;

namespace SRDocScanIDP
{
    internal static class Program
    {
        [DllImport("kernel32", SetLastError = true)]
        static extern IntPtr LoadLibrary(string lpFileName);
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            try
            {
                if (System.Diagnostics.Process.GetProcessesByName(System.Diagnostics.Process.GetCurrentProcess().ProcessName).Length > 1)
                {
                    MessageBox.Show("Another Instance of This Application Already Running" + Environment.NewLine +
                        "Multiple Instances Forbidden", "Application Errors", MessageBoxButtons.OK,
                                MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                }
                else
                {
                    Object oLic = null;
                    try
                    {
                        System.Reflection.Assembly asbLic = System.Reflection.Assembly.LoadFrom(System.Windows.Forms.Application.StartupPath + @"\LicLibGENNet.dll");
                        oLic = new object();
                    }
                    catch (DllNotFoundException dlx)
                    {
                        mGlobal.Write2Log("License module:" + dlx.Message);
                    }
                    catch (Exception ex)
                    {
                        mGlobal.Write2Log("License module:" + ex.Message);
                    }

                    if (oLic != null)
                    {
                        Application.SetHighDpiMode(HighDpiMode.PerMonitorV2);
                        Application.Run(new MDIMain());
                    }
                    else
                    {
                        MessageBox.Show("License module does not exist! Please contact your system administrator.");
                        Application.Exit();
                    }
                }

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

    }
}