using System;
using System.Text;

namespace Crm.Integration.Common
{
    public class PhoneMatchHelper
    {
        public static string NormalizePhoneNumber(string destination)
        {
            if(destination.StartsWith("skype:", StringComparison.InvariantCultureIgnoreCase))
            {
                return destination.Substring(6);
            }
            StringBuilder stringBuilder = new StringBuilder();
            foreach(char c in destination)
            {
                if (char.IsDigit(c) || (int)c == 43 || (int)c ==42 || (int)c == 35)
                    stringBuilder.Append(c);
            }
            return stringBuilder.ToString();
        }
    }
}
