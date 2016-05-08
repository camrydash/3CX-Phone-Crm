using Crm.Integration.Common;
using MyPhonePlugins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Crm.Integration
{
    public abstract class AbsCallNotifier
    {
        private IMyPhoneCallHandler _callHandler;

        protected readonly string _loggerFileName;
        protected readonly Crm3CXPluginService _service = null;

        public AbsCallNotifier(string loggerFileName, IMyPhoneCallHandler callHandler)
        {
            _loggerFileName = loggerFileName;

            _callHandler = callHandler;
            _callHandler.OnCallStatusChanged += _callHandler_OnCallStatusChanged;

            try
            {
                //initializer exception in creating service
                _service = new Crm3CXPluginService();
            }
            catch(Exception ex0)
            {
                LogHelper.Log(Environment.SpecialFolder.ApplicationData, _loggerFileName, ex0.Message);
            }
        }

        private void _callHandler_OnCallStatusChanged(object sender, CallStatus callInfo)
        {
            ThreadPool.QueueUserWorkItem(new WaitCallback(OnCallStatusChanged), (object)callInfo);
        }

        private void OnCallStatusChanged(object callData)
        {
            CallStatus callInfo = callData as CallStatus;
            if (callInfo == null) return;

            LogHelper.Log(Environment.SpecialFolder.ApplicationData, _loggerFileName, "CallStatusChanged: " + callInfo.State);
            LogHelper.Log(Environment.SpecialFolder.ApplicationData, _loggerFileName, Newtonsoft.Json.JsonConvert.SerializeObject(callInfo));

            if (_service == null)
            {
                LogHelper.Log(Environment.SpecialFolder.ApplicationData, _loggerFileName, "ServiceClient: NULL");
                return;
            }

            if (callInfo.Incoming)
            {
                if (callInfo.State == CallState.Connected)
                {
                    //Insert
                    _service.Insert(callInfo.CallID, "AbsCallNotifier", callInfo.OtherPartyNumber, callInfo.Incoming ? "Incoming" : "Outgoing",
                            callInfo.State.ToString(), 0, DateTime.UtcNow);
                }
            }
            else
            {
                if (callInfo.State == CallState.Dialing)
                {
                    _service.Insert(callInfo.CallID, "AbsCallNotifier", callInfo.OtherPartyNumber, callInfo.Incoming ? "Incoming" : "Outgoing",
                            callInfo.State.ToString(), 0, DateTime.UtcNow);
                }
            }

            _service.Update(callInfo.CallID, callInfo.State.ToString(), DateTime.UtcNow);

            LogHelper.Log(Environment.SpecialFolder.ApplicationData, _loggerFileName, callInfo.CallID);           
        }

        protected void MakeCall(string destination)
        {
            ThreadPool.QueueUserWorkItem(new WaitCallback(DoMakeCall), (object)destination);
        }
        
        private void DoMakeCall(object state)
        {
            string number = state as string;
            try
            {
                var destination = PhoneMatchHelper.NormalizePhoneNumber(number);           
                destination = destination.Insert(0, "9");
                LogHelper.Log(Environment.SpecialFolder.ApplicationData, _loggerFileName, "\tMaking call to '" + destination + "' - Original destination='" + number + "'");

                _callHandler.Show(Views.DialPad, ShowOptions.None);
                var callStatus = _callHandler.MakeCall(destination, MakeCallOptions.None);

                //LogHelper.Log(Environment.SpecialFolder.ApplicationData, _loggerFileName, Newtonsoft.Json.JsonConvert.SerializeObject(callStatus));
                //_service.Insert(callStatus.CallID, "AbsCallNotifier", destination, callStatus.Incoming ? "Incoming" : "Outgoing",
                //     callStatus.State.ToString(), 0, DateTime.UtcNow);

                //DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss")

                LogHelper.Log(Environment.SpecialFolder.ApplicationData, _loggerFileName, "\tExiting call");
            }
            catch(Exception ex)
            {
                LogHelper.Log(Environment.SpecialFolder.ApplicationData, _loggerFileName, ex.Message);
            }
        }
    }
}
