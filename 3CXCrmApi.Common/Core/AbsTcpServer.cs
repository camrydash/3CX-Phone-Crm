using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Crm.Integration.Common
{
    public abstract class AbsTcpServer : IDisposable
    {
        public abstract Dictionary<int, EndPoint> ConnectionsTable { get; }

        public abstract void Listen(IPEndPoint localEP);

        public abstract void Listen(string ipAddressStr, int portNo);

        public abstract void Listen(int portNo);

        public abstract EndPoint GetRemoteEndPoint(int index);

        public abstract void Send(int index, string data);

        public abstract void Close(int index);

        public abstract void Dispose();

        public delegate void ConnectHandler(int index, IPEndPoint remoteEP);

        public delegate void DisconnectHandler(int index);

        public delegate void DataArrivalHandler(int index, string data);
    }
}
