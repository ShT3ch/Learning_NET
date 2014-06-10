using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace IPtrace_to_AS
{
    /// <summary>
    /// A class to lookup whois information.
    /// </summary>
    public class Whois
    {
        public enum RecordType { domain, nameserver, registrar };

        /// <summary>
        /// retrieves whois information
        /// </summary>
        /// <param name="domainname">The registrar or domain or name server whose whois information to be retrieved</param>
        /// <param name="recordType">The type of record i.e a domain, nameserver or a registrar</param>
        /// <returns>The string containg the whois information</returns>
        public static string lookup(string domainname, RecordType recordType)
        {
            List<string> res = lookup(domainname, recordType, "whois.internic.net");
            string result = "";
            foreach (string st in res)
            {
                result += st + "\n";
            }
            return result;
        }        /// <summary>
        /// retrieves whois information
        /// </summary>
        /// <param name="domainname">The registrar or domain or name server whose whois information to be retrieved</param>
        /// <param name="recordType">The type of record i.e a domain, nameserver or a registrar</param>
        /// <param name="returnlist">use "whois.internic.net" if you dont know whoisservers</param>
        /// <returns>The string list containg the whois information</returns>
        public static List<string> lookup(string domainname, RecordType recordType, string whois_server_address)
        {
            if (whois_server_address == "")
                whois_server_address = "whois.internic.net";
            var tcp = new TcpClient();
            tcp.Connect(whois_server_address, 43);
            string strDomain = recordType.ToString() + " " + domainname + "\r\n";
            byte[] bytDomain = Encoding.ASCII.GetBytes(strDomain.ToCharArray());
            Stream s = tcp.GetStream();
            s.Write(bytDomain, 0, strDomain.Length);
            StreamReader sr = new StreamReader(tcp.GetStream(), Encoding.ASCII);
            string strLine = "";
            List<string> result = new List<string>();
            while (null != (strLine = sr.ReadLine()))
            {
                result.Add(strLine);
            }
            tcp.Close();
            return result;
        }
    }

    public class TraceRoute
    {
        private const string Data = "aaaaahellofromhelldeADBEEFaaaaaa";

        public static IEnumerable<IPAddress> GetTraceRoute(string hostNameOrAddress)
        {
            return GetTraceRoute(hostNameOrAddress, 1);
        }

        private static IEnumerable<IPAddress> GetTraceRoute(string hostNameOrAddress, int ttl)
        {
            Ping pinger = new Ping();
            PingOptions pingerOptions = new PingOptions(ttl, true);
            int timeout = 10000;
            byte[] buffer = Encoding.ASCII.GetBytes(Data);

            PingReply reply = pinger.Send(hostNameOrAddress, timeout, buffer, pingerOptions);

            List<IPAddress> result = new List<IPAddress>();
            if (reply.Status == IPStatus.Success)
            {
                result.Add(reply.Address);
            }
            else if (reply.Status == IPStatus.TtlExpired || reply.Status == IPStatus.TimedOut)
            {
                //add the currently returned address
                result.Add(reply.Address);
                //recurse to get the next address...
                result.AddRange(GetTraceRoute(hostNameOrAddress, ttl + 1));
            }
            else
            {
                //failure 
            }

            return result;
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
//            var show = new Whois();

            if (args.Count() == 2)
            {
                foreach (var s in TraceRoute.GetTraceRoute(args[1]))
                {
                    Console.WriteLine(s);
                }
            }
            else
            {
                Console.WriteLine("Something wrong with arguments. It must be exactly one internet adress.");
            }

           
            Console.ReadLine();
        }
    }
}
