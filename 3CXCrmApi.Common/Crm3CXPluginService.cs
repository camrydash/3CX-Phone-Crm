using Crm.Integration.Common.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;

namespace Crm.Integration.Common
{
    public class Crm3CXPluginService
    {
        private readonly string ConnectionString = "Data Source=3CXCrmPlugin.db";//CommonHelper.Read<string>("DatabasePath", "Data Source=3CXCrmPlugin.db");
        private readonly string LoggingPath = "db.log";// CommonHelper.Read<string>("db.log");

        private Crm3CXPhoneService.Crm3CXPhoneCallLogServiceClient _client;

        public Crm3CXPluginService()
        {
            // Specify a base address for the service
            EndpointAddress endpointAdress = new EndpointAddress("*/Crm3CXPhoneCallLogService.svc");
            // Create the binding to be used by the service - you will probably want to configure this a bit more
            BasicHttpBinding binding1 = new BasicHttpBinding();

            _client = new Crm3CXPhoneService.Crm3CXPhoneCallLogServiceClient(binding1, endpointAdress);
        }

        //public void InsertLocal(string callId, string caller, string destination, string callType, string callState, int callLength, string callDate)
        //{
        //    using (var db = new System.Data.SQLite.SQLiteConnection(ConnectionString))
        //    {
        //        var dbQuery = string.Empty;
        //        try
        //        {
        //            db.Open();
        //            using (System.Data.SQLite.SQLiteCommand command = new System.Data.SQLite.SQLiteCommand(db))
        //            {
        //                dbQuery = "INSERT INTO CallTransaction " + string.Format("VALUES ('{0}', '{1}', '{2}', '{3}', '{4}', '{5}', '{6}')"
        //                    , callId, caller, destination, callType, callState, callLength, callDate);
        //                command.CommandText = dbQuery;
        //                command.ExecuteNonQuery();
        //            }
        //        }
        //        catch (Exception exc)
        //        {
        //            LogHelper.Log(Environment.SpecialFolder.ApplicationData, LoggingPath, exc.Message);
        //            LogHelper.Log(Environment.SpecialFolder.ApplicationData, LoggingPath, dbQuery);
        //        }
        //    }
        //}

        //public void UpdateLocal(string callId, string callState, DateTime callEndDate)
        //{
        //    //strftime('%s', 'now', 'localtime') - strftime('%s', x.CallDate)
        //    using (var db = new System.Data.SQLite.SQLiteConnection(ConnectionString))
        //    {
        //        var dbQuery = string.Empty;
        //        try
        //        {
        //            db.Open();
        //            using (var command = new System.Data.SQLite.SQLiteCommand(db))
        //            {
        //                dbQuery = "UPDATE CallTransaction " + string.Format("SET CallState = '{0}', CallLength = {1} WHERE CallId = '{2}'"
        //                    , callState, "strftime('%s', '" + callEndDate.ToString("yyyy-MM-dd hh:mm:ss") + "') - strftime('%s', CallDate)", callId);
        //                command.CommandText = dbQuery;
        //                command.ExecuteNonQuery();
        //            }
        //        }
        //        catch (Exception exc)
        //        {
        //            LogHelper.Log(Environment.SpecialFolder.ApplicationData, LoggingPath, exc.Message);
        //            LogHelper.Log(Environment.SpecialFolder.ApplicationData, LoggingPath, dbQuery);
        //        }
        //    }
        //}

        public void Insert(string callId, string caller, string destination, string callType, string callState, int callLength, DateTime callDate)
        {
            try
            {
                _client.InsertCallLog(callId, caller, destination, callType, callState, callLength, callDate);

                LogHelper.Log(Environment.SpecialFolder.ApplicationData, LoggingPath, "Insert()");
            }
            catch(Exception ex_0)
            {
                LogHelper.Log(Environment.SpecialFolder.ApplicationData, LoggingPath, ex_0.Message);
            }
        }

        public void Update(string callId, string callState, DateTime callEndDate)
        {
            try
            {
                _client.UpdateCallLog(callId, callState, callEndDate);

                LogHelper.Log(Environment.SpecialFolder.ApplicationData, LoggingPath, "Update()");
            }
            catch (Exception ex_0)
            {
                LogHelper.Log(Environment.SpecialFolder.ApplicationData, LoggingPath, ex_0.Message);
            }
        }
    }
}
