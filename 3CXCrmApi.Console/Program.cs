using Crm.Integration.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Crm.Integration.XConsole
{
    class Program
    {
        private static TcpClient _client = new TcpClient();

        static void Call()
        {
            var command = new Crm.Integration.Common.Task._3CXTask()
            {
                Type = Crm.Integration.Common.Task._3CXTaskType.MakeCall,
                Data = "6148150441",
                IPOrigin = "127.0.0.1"
            };

            _client.Send(JsonConvert.SerializeObject(command));
            
        }

        [Flags]
        public enum Days : int
        {
            Monday = 1,
            Tuesday = 2,
            Wednesday = 4,
            Thursday = 8,
            Friday = 16,
            Saturday = 32,
            Sunday = 64,
            MondayToFriday = 31,
            All = 127,
            //None = 0
        }

        private static bool Overlap()
        {
            var availableDays = Enum.GetValues(typeof(Days)).Cast<Days>();

                foreach (var day in availableDays.Where(x=> ((Days)32).HasFlag(x)))
                {
                    foreach (var entityDay in availableDays.Where(x=> ((Days)32).HasFlag(x)))
                    {
                        if ((int)day == (int)entityDay)
                        {
                            Console.WriteLine("Match " + day.ToString());
                        }
                    }
                }
            
            return true;
        }
    

        static void Main(string[] args)
        {

            Overlap();
            Days x1 = Days.Monday | Days.Tuesday;
            Days x2 = Days.Monday | Days.Sunday;

            var avalableFlags = Enum.GetValues(typeof(Days)).Cast<Enum>();
            foreach(var flag in avalableFlags.Where(x1.HasFlag))
            {
                Console.WriteLine(flag.ToString());
            }

            //Crm3CXPluginService.Insert("5", "Danyal", "6148150441", "Outgoing", "Connected", 0, DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss"));
            //Thread.Sleep(5000);
            //Crm3CXPluginService.Update("5", "Ended", DateTime.Now);

            Console.WriteLine("\tConnecting to {0}:{1}", "127.0.0.1", 47260);
            _client.Connect("127.0.0.1", 47260);
            _client.OnDataArrival += _client_OnDataArrival;


            Console.WriteLine("Press K to call");
            while(Console.ReadLine() == "k")
            {
                Call();
            }

            Console.ReadKey();

            //Task.Factory.StartNew(() =>
            //{
            //    //installed on the users local machine
            //    TcpServer server = new TcpServer();
            //    server.OnDataArrival += Server_OnDataArrival;
            //    server.Listen(3780);
            //});

            //Task.Factory.StartNew(() =>
            //{
            //    Thread.Sleep(5000);
            //    //crm web
            //    //retreive ip address of user, and connect to ip and make connection with tcp server on that machine



            //    TcpClient client = new TcpClient();
            //    client.Connect("127.0.0.1", 3780);
            //    client.Send("127.0.0.1");
            //});


            //Console.ReadKey();
        }

        private static void _client_OnDataArrival(string data)
        {
            dynamic response = JsonConvert.DeserializeObject<dynamic>(data);

            if(!string.IsNullOrWhiteSpace(response.key.Value))
            {
                string raw_0 = response.key.Value.Split(':')[0];
                Console.WriteLine(raw_0);
            }

            Console.WriteLine("Received Data: " + data);
        }

        private static void Server_OnDataArrival(int index, string data)
        {
            throw new NotImplementedException();
        }
    }
}
