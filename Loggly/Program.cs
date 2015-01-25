using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using log4net;
using log4net.Repository.Hierarchy;

namespace Loggly
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            log4net.Config.XmlConfigurator.Configure();
            ILog Logger = LogManager.GetLogger(typeof(Program));

            //some sample custom properties
            log4net.GlobalContext.Properties["Component"] = "CurrentProgram";
            log4net.ThreadContext.Properties["IpAddress"] = "127.0.0.1";
            log4net.ThreadContext.Properties["AccountName"] = "jojo";
            log4net.ThreadContext.Properties["FlowContext"] = Guid.NewGuid().ToString();
            
            for (int i = 0; i < 1000; i++)
            {
                Logger.Debug("This is a simple test made by console - git" + i);    
            }
            Console.WriteLine("done");
            Console.ReadLine();
        }

        private static void SendMessage()
        {
            
        }
    }
}
