using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;

namespace IPtrace_to_AS
{
    public class Whois
    {
        private List<string> WhoisServers;
        public Whois()
        {
            System.IO.StreamReader reader;
            WhoisServers = new List<string>();

            try
            {
                reader = new System.IO.StreamReader(path: ".\\whoises");
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
                    catch (Exception ex)
                    {
                        Console.WriteLine("Maybe you forgot format of config file? :)");
                        Console.WriteLine("Exception said: '{0}'", ex.Message);
                    }
                }
            }
            catch (Exception)
            {
                Console.WriteLine("maybe you forgot config file?");   
            }
        }

        public string lookup(string domainname)
        {
            string answer = "";

            foreach (var server in WhoisServers)
            {
                Console.WriteLine("Asking for '{0}' from :'{1}'",domainname,server);
                var temp = lookup(domainname, server);
                if (answer.Equals(temp) || temp.Equals(""))
                {
                    //its nothing to do: всё тлен.
                }
                else
                {
                    return temp;
                }
            }

            return answer;
        }        

        private string lookup(string domainname, string whois_server_address)
        {
            var tcp = new TcpClient();
            Stream s;

            string strDomain = domainname + "\r\n";
            byte[] bytDomain = Encoding.ASCII.GetBytes(strDomain.ToCharArray());

            try
            {
                tcp.Connect(whois_server_address, 43);
                s = tcp.GetStream();
                s.Write(bytDomain, 0, strDomain.Length);
                StreamReader sr = new StreamReader(tcp.GetStream(), Encoding.ASCII);
                //forSN@list.ru
                var regex_ASfind = new List<Regex>();
                regex_ASfind.Add(new Regex("origin:\\s+(AS\\d+)\\s"));
                regex_ASfind.Add(new Regex("mnt-routes:\\s+(AS\\d+)-MNT"));
                regex_ASfind.Add(new Regex("OriginAS:\\s+(AS\\d+)\\s"));

                var regex_InfoFields = new Dictionary<Regex,string>();
                regex_InfoFields.Add(new Regex("netname:\\s+([\\w-_\\d]+)\\s")," NetName: ");
                regex_InfoFields.Add(new Regex("City:\\s(\\w+)\\s"), " City: " );
                regex_InfoFields.Add(new Regex("Country:\\s+(\\w+)\\s"), " Country: ");
                regex_InfoFields.Add(new Regex("country:\\s+(\\w+)\\s"), " Country: ");
//                
                var temp = sr.ReadToEnd();
                foreach (var reg in regex_ASfind)
                {
                    var matchedAS = reg.Match(temp);
                    var infoFields = "";

                    foreach (var regexInfoField in regex_InfoFields.Keys)
                    {
                        var matched = regexInfoField.Match(temp);
                        if (matched.Success)
                        {
                            if (!infoFields.Equals(""))
                            {
                                infoFields += ";   " + regex_InfoFields[regexInfoField] + matched.Groups[1];
                            }
                            else
                            {
                                infoFields += regex_InfoFields[regexInfoField] + matched.Groups[1];
                            }
                        }
                    }


                    if (matchedAS.Success)
                    {
                        Console.WriteLine("AS matched at '{0}'", whois_server_address);
                        return matchedAS.Groups[1].Value + infoFields;
                    }
                }

                return "";

            }
            catch (Exception ex)
            {
                Console.WriteLine("Host unreacheble. Please, check your net connection.");
            }

            return "";
        }
    }

    public class TraceRoute
    {
        private const string Data = "aaaaahellofromhelldeADBEEFaaaaaa";

        private static bool IsHostReacheble(string host)
        {
            Ping pinger = new Ping();
            PingOptions pingerOptions = new PingOptions(60, true);
            int timeout = 2500;
            byte[] buffer = Encoding.ASCII.GetBytes(Data);

            PingReply reply = pinger.Send(host, timeout, buffer, pingerOptions);

            if (reply.Status == IPStatus.Success)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static IEnumerable<IPAddress> GetTraceRoute(string hostNameOrAddress)
        {
            if (IsHostReacheble(hostNameOrAddress))
            {
                return GetTraceRoute(hostNameOrAddress, 1);
            }
            else
            {
                Console.WriteLine("Host unreacheble. :(");
            }
            return new List<IPAddress>();
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
                if (reply.Status == IPStatus.TimedOut) result.Add(null);
                if (reply.Address != null)
                {
                    result.Add(reply.Address);
                    Console.WriteLine("\tDiscovered new IP: {0}", reply.Address);
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
            if (args.Count() == 1)
            {
                var show = new Whois();

                var points = TraceRoute.GetTraceRoute(args[0]);
                
                var answer = new Dictionary<string,HashSet<string>>();

                foreach (var ipAddress in points)
                {
                    var index = 0;
                    if (ipAddress != null)
                    {
                        var temp = show.lookup(ipAddress.ToString());
                        if ((answer.ContainsKey(temp)) && !temp.Equals(""))
                        {

                        }
                        else if (!answer.ContainsKey(temp))
                        {
                            answer[temp] = new HashSet<string>();
                            answer[temp].Add(ipAddress.ToString());
                        }
                    }
                    else
                    {
                        answer[index.ToString()] = new HashSet<string>();
                        answer[(index++).ToString()].Add("lost packet");
                    }
                }
                foreach (var ASNumber in answer.Keys)
                {
                    Console.WriteLine(ASNumber);
                    foreach (var adress in answer[ASNumber])
                    {
                        Console.WriteLine("\t{0}",adress);
                    }
                }
                Console.ReadLine();
            }
            else
            {
                Console.WriteLine("something wrong with arguments");
            }
            Console.ReadLine();
        }
    }
}
