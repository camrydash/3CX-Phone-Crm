using Crm.Integration.Common;
using Crm.Integration.Common.Task;
using MyPhonePlugins;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Crm.Integration
{
    [CRMPluginLoader]
    public class Plugin3CX : AbsCallNotifier
    {
        private TcpServer _tcpServer;
        private static Plugin3CX instance;

        [CRMPluginInitializer]
        public static void Loader(IMyPhoneCallHandler callHandler)
        {
            instance = new Plugin3CX(callHandler);
        }

        private Plugin3CX(IMyPhoneCallHandler callHandler) 
            : base("3CXCrm.log", callHandler)
        {
            try
            {
                LogHelper.Log(Environment.SpecialFolder.ApplicationData, _loggerFileName, "Listening on port 47260");

                _tcpServer = new TcpServer();
                _tcpServer.Listen(47260);
                _tcpServer.OnDataArrival += _tcpServer_OnDataArrival;
                _tcpServer.OnConnect += _tcpServer_OnConnect;
                _tcpServer.OnDisconnect += _tcpServer_OnDisconnect;
            }
            catch(Exception ex_0)
            {
                MessageBox.Show(ex_0.Message);
                LogHelper.Log(Environment.SpecialFolder.ApplicationData, _loggerFileName, ex_0.Message);
            }
        }

        private void _tcpServer_OnDisconnect(int index)
        {
            LogHelper.Log(Environment.SpecialFolder.ApplicationData, _loggerFileName, "TcpServer Disconnected " + "(" + index + ")");
        }

        private void _tcpServer_OnConnect(int index, System.Net.IPEndPoint remoteEP)
        {
            LogHelper.Log(Environment.SpecialFolder.ApplicationData, _loggerFileName, string.Format("Connected: {0}:{1}", remoteEP.Address, remoteEP.Port));
        }

        private void _tcpServer_OnDataArrival(int index, string data)
        {
            var commandText = data;
            _3CXTask task = null;
            try
            {
                LogHelper.Log(Environment.SpecialFolder.ApplicationData, _loggerFileName, "Data Received: " + data);
                task = JsonConvert.DeserializeObject<_3CXTask>(data);
                if (task == null)
                    throw new ApplicationException("Unable to decode data command");
                if (task.Type == _3CXTaskType.MakeCall)
                {
                    if (string.IsNullOrEmpty(task.Data))
                        throw new ApplicationException("Destination number is empty");
                    this.MakeCall(task.Data);
                }
                else if (task.Type == _3CXTaskType.Handshake)
                {
                    //Do Nothing
                }
            }
            catch(Exception ex_0)
            {
                LogHelper.Log(Environment.SpecialFolder.ApplicationData, _loggerFileName, ex_0.Message);
            }
            finally
            {
                //var response = "response";

                //DateTime dateTime = DateTime.Now.ToUniversalTime();
                //StringBuilder stringBuilder = new StringBuilder();
                //stringBuilder.AppendLine("HTTP/1.1 200 OK");
                //stringBuilder.AppendLine(string.Format("Content-Length: {0}", (object)response.Length));
                //stringBuilder.AppendLine("Content-Type: text/html");
                //stringBuilder.AppendLine("Cache-Control: no-cache");
                //stringBuilder.AppendLine("Cache-Control: no-store");
                //stringBuilder.AppendLine(string.Format("Last-Modified: {0}", (object)dateTime.ToString("R")));
                //stringBuilder.AppendLine("Accept-Ranges: bytes");
                //stringBuilder.AppendLine(string.Format("Date: {0}", (object)dateTime.ToString("R")));
                //stringBuilder.AppendLine();
                //stringBuilder.AppendLine(response);

                //Update this to later on send callhistory id
                if (task != null)
                {
                    var response = new
                    {
                        key = task.IPOrigin,
                        data = ((int)HttpStatusCode.OK).ToString()
                    };

                    this._tcpServer.Send(index, JsonConvert.SerializeObject(response));
                }
            }
        }
    }
}
