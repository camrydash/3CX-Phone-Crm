using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Crm.Integration.Common
{
    public class TcpClient : AbsTcpClient
    {
        private bool isClosed = true;
        private byte[] receiveState = new byte[1024];
        private IPEndPoint remoteEP;
        private Socket workerSocket;
        private ManualResetEvent finishSend;
        private bool isSync;
        private string syncReceivedData;

        public override IPEndPoint RemoteEndPoint
        {
            get
            {
                if (this.isClosed)
                    return (IPEndPoint)null;
                return this.remoteEP;
            }
        }

        public event AbsTcpClient.DisconnectHandler OnDisconnect;

        public event AbsTcpClient.DataArrivalHandler OnDataArrival;

        public TcpClient()
        {
            this.finishSend = new ManualResetEvent(false);
        }

        ~TcpClient()
        {
            if (this.isClosed)
                return;
            this.Dispose();
        }

        private void receiveCallback(IAsyncResult ar)
        {
            lock (this)
            {
                if (this.isClosed)
                    return;
                int local_0;
                try
                {
                    local_0 = this.workerSocket.EndReceive(ar);
                }
                catch (SocketException exception_1)
                {
                    local_0 = 0;
                }
                if (local_0 > 0)
                {
                    if (this.isSync)
                    {
                        this.syncReceivedData = Encoding.Default.GetString(this.receiveState, 0, local_0);
                        this.finishSend.Set();
                    }
                    else if (this.OnDataArrival != null)
                        this.OnDataArrival(Encoding.Default.GetString(this.receiveState, 0, local_0));
                    try
                    {
                        this.workerSocket.BeginReceive(this.receiveState, 0, this.receiveState.Length, SocketFlags.None, new AsyncCallback(this.receiveCallback), (object)null);
                    }
                    catch (SocketException exception_0)
                    {
                        this.isClosed = true;
                        this.workerSocket.Close();
                        if (this.OnDisconnect == null)
                            return;
                        this.OnDisconnect();
                    }
                }
                else
                {
                    this.isClosed = true;
                    this.workerSocket.Close();
                    if (this.OnDisconnect == null)
                        return;
                    this.OnDisconnect();
                }
            }
        }

        private void sendCallback(IAsyncResult ar)
        {
            lock (this)
            {
                try
                {
                    if (this.isClosed)
                        return;
                    this.workerSocket.EndSend(ar);
                }
                catch (SocketException exception_0)
                {
                }
            }
        }

        private void sendTimeoutElapsed(object state)
        {
            this.syncReceivedData = "";
            this.finishSend.Set();
        }

        private void internalClose()
        {
            lock (this)
            {
                this.isClosed = true;
                this.workerSocket.Close();
            }
        }

        public override void Connect(IPEndPoint remoteEP)
        {
            if (!this.isClosed)
                throw new ArgumentException("Socket is already connected.");
            this.remoteEP = remoteEP;
            this.workerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            this.workerSocket.Connect((EndPoint)remoteEP);
            this.isClosed = false;
            try
            {
                this.workerSocket.BeginReceive(this.receiveState, 0, this.receiveState.Length, SocketFlags.None, new AsyncCallback(this.receiveCallback), (object)null);
            }
            catch (SocketException ex)
            {
                this.isClosed = true;
                this.workerSocket.Close();
                if (this.OnDisconnect == null)
                    return;
                this.OnDisconnect();
            }
        }

        public override void Connect(string ipAddressStr, int portNo)
        {
            this.Connect(new IPEndPoint(IPAddress.Parse(ipAddressStr), portNo));
        }

        public override void Send(string data)
        {
            if (this.isClosed)
                throw new ObjectDisposedException(this.ToString(), "At Send method. Socket is closed.");
            byte[] bytes = Encoding.Default.GetBytes(data);
            try
            {
                this.workerSocket.BeginSend(bytes, 0, bytes.Length, SocketFlags.None, new AsyncCallback(this.sendCallback), (object)null);
            }
            catch (SocketException ex)
            {
                this.isClosed = true;
                this.workerSocket.Close();
                if (this.OnDisconnect == null)
                    return;
                this.OnDisconnect();
            }
        }

        public override string SendSync(string data, int timeOut)
        {
            this.isSync = true;
            this.finishSend.Reset();
            try
            {
                this.Send(data);
                using (new Timer(new TimerCallback(this.sendTimeoutElapsed), (object)null, timeOut, timeOut))
                {
                    this.finishSend.WaitOne();
                    return this.syncReceivedData;
                }
            }
            finally
            {
                this.isSync = false;
            }
        }

        public override void Close()
        {
            if (this.isClosed)
                return;
            this.internalClose();
        }

        public override void Dispose()
        {
            if (!this.isClosed)
                this.internalClose();
            GC.SuppressFinalize((object)this);
        }
    }
}
