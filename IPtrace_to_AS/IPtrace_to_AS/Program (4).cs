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
        private List<string> WhoisServers;
        Whois()
        {
            var reader = new System.IO.StreamReader(path: ".\\whoises");
            WhoisServers = new List<string>();
            while (!reader.EndOfStream)
            {
                try
                {
                    var serv = reader.ReadLine();
                    if (serv.Contains("\\s"))
                    {
                        throw new Exception();
                    }
                    else
                    {
                        WhoisServers.Add(serv);
                    }
                }
                catch(Exception ex)
                {
                    Console.WriteLine("Maybe you forgot format of config file? :)");
                    Console.WriteLine("Exception said: '{0}'",ex.Message);
                }
            }
        }

        /// <summary>
        /// retrieves whois information
        /// </summary>
        /// <param name="domainname">The registrar or domain or name server whose whois information to be retrieved</param>
        /// <param name="recordType">The type of record i.e a domain, nameserver or a registrar</param>
        /// <returns>The string containg the whois information</returns>
        public static string lookup(string domainname)
        {
            var res = lookup(domainname,"whois.internic.net");

            return res;
        }        /// <summary>
        /// retrieves whois information
        /// </summary>
        /// <param name="domainname">The registrar or domain or name server whose whois information to be retrieved</param>
        /// <param name="recordType">The type of record i.e a domain, nameserver or a registrar</param>
        /// <param name="returnlist">use "whois.internic.net" if you dont know whoisservers</param>
        /// <returns>The string list containg the whois information</returns>
        private static string lookup(string domainname, string whois_server_address)
        {
            var tcp = new TcpClient();
            tcp.Connect(whois_server_address, 43);
            Stream s = tcp.GetStream();

            string strDomain = domainname + "\r\n";
            byte[] bytDomain = Encoding.ASCII.GetBytes(strDomain.ToCharArray());
            

            s.Write(bytDomain, 0, strDomain.Length);
            StreamReader sr = new StreamReader(tcp.GetStream(), Encoding.ASCII);

            return sr.ReadToEnd();
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
            int timeout = 1000;
            byte[] buffer = Encoding.ASCII.GetBytes(Data);

            PingReply reply = pinger.Send(hostNameOrAddress, timeout, buffer, pingerOptions);

            List<IPAddress> result = new List<IPAddress>();
            if (reply.Status == IPStatus.Success)
            {
                if (reply.Address != null)
                {
                    result.Add(reply.Address);
                    Console.WriteLine("Discovered new IP: {0}", reply.Address);
                }
            }
            else if (reply.Status == IPStatus.TtlExpired || reply.Status == IPStatus.TimedOut)
            {
                //add the currently returned address
                if (reply.Address != null)
                {
                    result.Add(reply.Address);
                    Console.WriteLine("Discovered new IP: {0}", reply.Address);
                }
                else
                {
                    Console.WriteLine("Lost packet on ttl = {0} with verdict {1}",ttl,reply.Status);
                }
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
            var show = new Whois();

//            Console.WriteLine(Whois.lookup(@"google.com",Whois.RecordType.domain));

            if (args.Count() == 1)
            {
                foreach (var s in TraceRoute.GetTraceRoute(args[0]))
                {
//                    Console.WriteLine(s);
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
