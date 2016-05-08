using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;

namespace Crm.Integration.Common
{
    public class CommonHelper
    {
        private static string Read(string key, string defaultValue)
        {
            //if (!ConfigurationManager.AppSettings.AllKeys.Contains(key))
            //  throw new ArgumentException("Key does not exist in configuration file");

            return ConfigurationManager.AppSettings.Get(key) ?? defaultValue;
        }

        public static T Read<T>(string key, T defaultValue = default(T))
        {
            var value = default(T);
            try
            {
                return (T)Convert.ChangeType(Read(key, defaultValue.ToString()), typeof(T));
            }
            catch { }
            return value;
        }
    }
}
