using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Crm.Integration.Common
{
    public class TcpServer : AbsTcpServer
    {
        private Dictionary<int, EndPoint> connectionsTable;
        private Dictionary<int, Socket> workerSocketTable;
        private Socket listenerSocket;
        private ManualResetEvent outsideCallback;
        private ManualResetEvent outsideClose;
        private bool isDisposed;
        private static int uniqueKey;

        public override Dictionary<int, EndPoint> ConnectionsTable
        {
            get
            {
                if (this.isDisposed)
                    return (Dictionary<int, EndPoint>)null;
                return new Dictionary<int, EndPoint>((IDictionary<int, EndPoint>)this.connectionsTable);
            }
        }

        public event ConnectHandler OnConnect;

        public event DisconnectHandler OnDisconnect;

        public event DataArrivalHandler OnDataArrival;

        public TcpServer()
        {
            this.connectionsTable = new Dictionary<int, EndPoint>();
            this.workerSocketTable = new Dictionary<int, Socket>();
            this.outsideCallback = new ManualResetEvent(true);
            this.outsideClose = new ManualResetEvent(true);
        }

        ~TcpServer()
        {
            if (this.isDisposed)
                return;
            this.Dispose();
        }

        private void acceptCallback(IAsyncResult ar)
        {
            if (this.isDisposed)
                return;
            int num = Interlocked.Increment(ref TcpServer.uniqueKey);
            Socket socket = this.listenerSocket.EndAccept(ar);
            this.workerSocketTable.Add(num, socket);
            this.connectionsTable.Add(num, socket.RemoteEndPoint);
            this.listenerSocket.BeginAccept(new AsyncCallback(this.acceptCallback), (object)null);
            if (this.OnConnect != null)
                this.OnConnect(num, (IPEndPoint)socket.RemoteEndPoint);
            ReceiveState receiveState = new ReceiveState();
            receiveState.workerSocketIndex = num;
            try
            {
                socket.BeginReceive(receiveState.buffer, 0, receiveState.buffer.Length, SocketFlags.None, new AsyncCallback(this.receiveCallback), (object)receiveState);
            }
            catch (SocketException ex)
            {
                this.workerSocketTable.Remove(receiveState.workerSocketIndex);
                this.connectionsTable.Remove(receiveState.workerSocketIndex);
                socket.Close();
                if (this.OnDisconnect == null)
                    return;
                this.OnDisconnect(receiveState.workerSocketIndex);
            }
        }

        private void receiveCallback(IAsyncResult ar)
        {
            try
            {
                this.outsideCallback.Reset();
                this.outsideClose.WaitOne();
                ReceiveState receiveState = (ReceiveState)ar.AsyncState;
                if (!this.workerSocketTable.ContainsKey(receiveState.workerSocketIndex))
                    return;
                Socket socket = this.workerSocketTable[receiveState.workerSocketIndex];
                int count;
                try
                {
                    count = socket.EndReceive(ar);
                }
                catch (SocketException ex)
                {
                    count = 0;
                }
                if (count > 0)
                {
                    if (this.OnDataArrival != null)
                        this.OnDataArrival(receiveState.workerSocketIndex, Encoding.Default.GetString(receiveState.buffer, 0, count));
                    try
                    {
                        socket.BeginReceive(receiveState.buffer, 0, receiveState.buffer.Length, SocketFlags.None, new AsyncCallback(this.receiveCallback), (object)receiveState);
                    }
                    catch (SocketException ex)
                    {
                        this.workerSocketTable.Remove(receiveState.workerSocketIndex);
                        this.connectionsTable.Remove(receiveState.workerSocketIndex);
                        socket.Close();
                        if (this.OnDisconnect == null)
                            return;
                        this.OnDisconnect(receiveState.workerSocketIndex);
                    }
                }
                else
                {
                    this.workerSocketTable.Remove(receiveState.workerSocketIndex);
                    this.connectionsTable.Remove(receiveState.workerSocketIndex);
                    socket.Close();
                    if (this.OnDisconnect == null)
                        return;
                    this.OnDisconnect(receiveState.workerSocketIndex);
                }
            }
            finally
            {
                this.outsideCallback.Set();
            }
        }

        private void sendCallback(IAsyncResult ar)
        {
            try
            {
                this.outsideCallback.Reset();
                this.outsideClose.WaitOne();
                int key = (int)ar.AsyncState;
                if (!this.workerSocketTable.ContainsKey(key))
                    return;
                this.workerSocketTable[key].EndSend(ar);
            }
            catch (SocketException ex)
            {
            }
            finally
            {
                this.outsideCallback.Set();
            }
        }

        private void internalClose(int index)
        {
            try
            {
                this.outsideClose.Reset();
                this.outsideCallback.WaitOne();
                if (!this.workerSocketTable.ContainsKey(index))
                    throw new ArgumentException(string.Format("At Close method, index {0} does not exist in the collection.", (object)index));
                this.workerSocketTable[index].Close();
                this.workerSocketTable.Remove(index);
                this.connectionsTable.Remove(index);
            }
            finally
            {
                this.outsideClose.Set();
            }
        }

        public override void Listen(IPEndPoint localEP)
        {
            if (this.isDisposed)
                throw new ObjectDisposedException(this.ToString(), "At Listen method.");
            this.listenerSocket = new Socket(localEP.Address.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            this.listenerSocket.Bind((EndPoint)localEP);
            this.listenerSocket.Listen(10);
            this.listenerSocket.BeginAccept(new AsyncCallback(this.acceptCallback), (object)null);
        }

        public override void Listen(string ipAddressStr, int portNo)
        {
            this.Listen(new IPEndPoint(IPAddress.Parse(ipAddressStr), portNo));
        }

        public override void Listen(int portNo)
        {
            this.Listen(new IPEndPoint(IPAddress.Any, portNo));
        }

        public override EndPoint GetRemoteEndPoint(int index)
        {
            if (this.connectionsTable.ContainsKey(index))
                return this.connectionsTable[index];
            throw new ArgumentException(string.Format("At GetRemoteEndPoint method, index {0} does not exist in the collection.", (object)index));
        }

        public override void Send(int index, string data)
        {
            if (this.isDisposed)
                throw new ObjectDisposedException(this.ToString(), "At Send method.");
            if (!this.workerSocketTable.ContainsKey(index))
                throw new ArgumentException(string.Format("At Send method, index {0} does not exist in the collection.", (object)index));
            byte[] bytes = Encoding.Default.GetBytes(data);
            Socket socket = this.workerSocketTable[index];
            try
            {
                socket.BeginSend(bytes, 0, bytes.Length, SocketFlags.None, new AsyncCallback(this.sendCallback), (object)index);
            }
            catch (SocketException ex)
            {
            }
        }

        public override void Close(int index)
        {
            if (this.isDisposed)
                return;
            this.internalClose(index);
        }

        public override void Dispose()
        {
            this.isDisposed = true;
            this.listenerSocket.Close();
            foreach (KeyValuePair<int, Socket> keyValuePair in new Dictionary<int, Socket>((IDictionary<int, Socket>)this.workerSocketTable))
                this.internalClose(keyValuePair.Key);
            GC.SuppressFinalize((object)this);
        }
    }
}
