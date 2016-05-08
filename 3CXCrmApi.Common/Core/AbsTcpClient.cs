using System;
using System.Net;

namespace Crm.Integration.Common
{
    public abstract class AbsTcpClient : IDisposable
    {
        public abstract IPEndPoint RemoteEndPoint { get; }

        public abstract void Connect(IPEndPoint remoteEP);

        public abstract void Connect(string ipAddressStr, int portNo);

        public abstract void Send(string data);

        public abstract string SendSync(string data, int timeOut);

        public abstract void Close();

        public abstract void Dispose();

        public delegate void DisconnectHandler();

        public delegate void DataArrivalHandler(string data);
    }
}
