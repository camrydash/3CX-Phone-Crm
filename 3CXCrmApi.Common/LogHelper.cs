using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crm.Integration.Common
{
    public class LogHelper
    {
        private static object locking = new object();

        public static void Log(Environment.SpecialFolder specialFolder, string fileName, string text)
        {
            lock(locking)
            {
                try
                {
                    string local_0 = Path.Combine(Environment.GetFolderPath(specialFolder), "3CX CRM Integration\\Logs");
                    if (!Directory.Exists(local_0))
                        Directory.CreateDirectory(local_0);
                    File.AppendAllText(Path.Combine(local_0, fileName), DateTime.Now.ToString() + " - " + text + Environment.NewLine);
                }
                catch
                {

                }
            }
        }
    }
}
