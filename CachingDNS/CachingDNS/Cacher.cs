using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Timers;
using Timer = System.Timers.Timer;
using System.Net.Sockets;

namespace CachingDNS
{
    class Cacher
    {
        private Timer timer;

        private Dictionary<DNSPacket, DNSPacket> Ask2Answer;

        private InetClient nc;


        public Cacher()
        {
            Ask2Answer = new Dictionary<DNSPacket, DNSPacket>();

            timer = new Timer(1000);
            timer.Elapsed += CleanUpOldAnswers;
            timer.Start();
        }

        public InetClient Nc
        {
            get { return nc; }
            set { nc = value; }
        }

        public byte[] GetAnswer(byte[] input)
        {
            var packet = new DNSPacket(input);
            var hash = packet.GetHashCode();

            byte[] ans;

            if (Ask2Answer.ContainsKey(packet))
            {
                Console.WriteLine("It`s already in base: {0}",packet.allQueries());
                ans = Ask2Answer[packet].makeAnswer(packet);
            }
            else
            {
                ans = nc.makeQuery(input);
                var newPacketans = new DNSPacket(ans);
                if (newPacketans.IsPacketSucces())Ask2Answer.Add(packet, newPacketans);
            }

            return ans;
        }

        void CleanUpOldAnswers(object sender, ElapsedEventArgs e)
        {
            GarbageCollector();
        }

        private void GarbageCollector()
        {
            lock (Ask2Answer)
            {   
                var candidates = new List<DNSPacket>();
                foreach (var dnsPacketPair in Ask2Answer)
                {
                    if (dnsPacketPair.Value.IsPacketAnswer())
                    {
                        if (dnsPacketPair.Value.getMinTTL()%10 == 0)
                        {
                            Console.WriteLine("for query {0} ttl={1}", dnsPacketPair.Value.allQueries(), dnsPacketPair.Value.getMinTTL());
                        }

                        if (dnsPacketPair.Value.getMinTTL() > 0)
                        {
                            dnsPacketPair.Value.SetTTL(dnsPacketPair.Value.getMinTTL() - 1);
                        }
                        else
                        {
                            candidates.Add(dnsPacketPair.Key);
                        }
                    }
                }
                foreach (var candidate in candidates)
                {
                    Console.WriteLine("{0} removed",Ask2Answer[candidate].allQueries());
                    Ask2Answer.Remove(candidate);

                }
            }
//            Console.WriteLine("hello!");
//            Thread.Sleep(4000);
        }
    }
}
