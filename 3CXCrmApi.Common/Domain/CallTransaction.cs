using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Crm.Integration.Common.Domain
{
    public class CallTransaction
    {
        public string CallId { get; set; }
        public string Caller { get; set; }
        public string Destination { get; set; }
        public string CallType { get; set; }
        public string CallState { get; set; }
        public int CallLength { get; set; }
        public string CallDate { get; set; }

    }
}
