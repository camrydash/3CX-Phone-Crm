using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;

namespace Crm.Integration.Installation
{
    class Program
    {
        private static string[] _copyFiles = new string[]
        {
            "Crm.Integration.Common.dll",
            "Crm.Integration.dll",
            "System.Data.SQLite.dll",
            "3CXCrmPlugin.db"
        };

        private static string _installationDirectory = @"C:\ProgramData\3CXPhone for Windows\PhoneApp";
        private static string _pluginConfigurationFile = Path.Combine(_installationDirectory, "3CXWin8Phone.user.config");
        private static string configurationBackup = Path.Combine(_installationDirectory, "3CXWin8Phone.user.config.bak");

        private static List<string> _installedFiles = new List<string>();

        static void SetError(string error, bool osreturn = true)
        {
            MessageBox.Show(error);
            if(osreturn)
                return;
        }

        static void CloseProcess(params string[] processes)
        {
            foreach(var processName in processes)
            {
                Process[] process = Process.GetProcessesByName(processName);
                if (process.Length > 0)
                {
                    try
                    {
                        process[0].Kill();
                        process[0].WaitForExit();
                    }
                    catch { }
                }
            }
        }

        static void Main(string[] args)
        {
            if(!IsAdministrator())
            {
                MessageBox.Show("3CX_Crm_Plugin requires administrator rights to be installed. Please run as administrator.");
                return;
            }

            if(!Directory.Exists(_installationDirectory) || !File.Exists(_pluginConfigurationFile))
            {
                MessageBox.Show("3CX is not installed on system or is corrupted");
                return;
            }

            if (MessageBox.Show("Close 3CXPhone and continue installation?", "3CX_Crm_Plugin", MessageBoxButtons.YesNo) == DialogResult.No)
                return;

            //Make sure 3CX is closed before installation
            CloseProcess("3CX Plugins Manager", "3CXWin8Phone");

            bool installationError = false;
            string errorMessage = "";
            try
            {
                Backup(); 
                CopyFiles();
                UpdateConfiguration();
            }
            catch(Exception exc)
            {
                errorMessage = exc.Message;
                installationError = true;
            }

            if (installationError)
            {
                Restore();
                MessageBox.Show(errorMessage, "Installation Failed");
            }
            else
            {
                MessageBox.Show("Installation Complete");
                //Process.Start(Path.Combine(_installationDirectory, "3CXWin8Phone.exe"));
            }
        }

        private static void Restore()
        {
            //delete any of the files left over by the partial installation
            foreach (var delete_0 in _installedFiles)
            {
                try
                {
                    File.Delete(Path.Combine(_installationDirectory, delete_0));
                }
                catch { }
            }

            //restore original configuration
            if(File.Exists(configurationBackup))
                File.Copy(configurationBackup, Path.Combine(_installationDirectory, "3CXWin8Phone.user.config"), true);
        }

        private static void Backup()
        {
            //make a backup of the original confuration file    
            if(!File.Exists(configurationBackup))   
                File.Copy(_pluginConfigurationFile, configurationBackup);
        }

        private static void UpdateConfiguration()
        {
            var x_document = XDocument.Load(_pluginConfigurationFile);

            var plugin_attribute = x_document.Elements("appSettings").Elements()
            .Where(x => x.FirstAttribute.Name.LocalName.Equals("key", StringComparison.InvariantCultureIgnoreCase)
               && x.FirstAttribute.Value == "CRMPlugin").SingleOrDefault();

            if (plugin_attribute != null)
            {
                var attribute_val_0 = plugin_attribute.Attribute(XName.Get("value"));
                if (!string.IsNullOrWhiteSpace(attribute_val_0.Value))
                {
                    if (attribute_val_0.Value.Contains("Crm.Integration"))
                        return;
                }
                else
                {
                    attribute_val_0.Value = "";
                }

                if (attribute_val_0.Value.Split(',').Length > 0)
                    plugin_attribute.SetAttributeValue(XName.Get("value"), attribute_val_0.Value + "," + "Crm.Integration");
                else
                    plugin_attribute.SetAttributeValue(XName.Get("value"), "Crm.Integration");

                x_document.Save(_pluginConfigurationFile);                             
            }
        }

        private static void CopyFiles()
        {
            for (int index = 0; index < _copyFiles.Length; index++)
            {
                var file = _copyFiles[index];

                var copy_0 = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, file);
                if (!File.Exists(copy_0))
                    throw new ApplicationException("Missing file: " + file);

                bool overwrite = true;
                var copyPath = Path.Combine(_installationDirectory, file);
                File.Copy(copy_0, copyPath, overwrite);
                _installedFiles.Add(file);
            }
        }

        public static bool IsAdministrator()
        {
            return (new WindowsPrincipal(WindowsIdentity.GetCurrent()))
                    .IsInRole(WindowsBuiltInRole.Administrator);
        }
    }
}
