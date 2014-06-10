using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


namespace CachingDNS
{
    public static class bynaryUtils
    {
//        public static string Reverse(string s)
//        {
//            char[] charArray = s.ToCharArray();
//            Array.Reverse(charArray);
//            return new string(charArray);
//        }

        public static byte[] StringToByteArray(string hex)
        {
            return Enumerable.Range(0, hex.Length)
                             .Where(x => x % 2 == 0)
                             .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                             .ToArray();
        }
    }

    class main
    {
        static void Main(string[] args)
        {

            var server = new Cacher();

            var serv = new InetClient(server);

            var servThread = new Thread(serv.StartListener);
            
            servThread.Start();
            //
            Thread.Sleep(10000);
            //
            var test =
                new DNSPacket(
                    bynaryUtils.StringToByteArray(
                        "000681800001000b0004000406676f6f676c6503636f6d0000010001c00c000100010000012c0004adc22fc6c00c000100010000012c0004adc22fc7c00c000100010000012c0004adc22fc8c00c000100010000012c0004adc22fc9c00c000100010000012c0004adc22fcec00c000100010000012c0004adc22fc0c00c000100010000012c0004adc22fc1c00c000100010000012c0004adc22fc2c00c000100010000012c0004adc22fc3c00c000100010000012c0004adc22fc4c00c000100010000012c0004adc22fc5c00c0002000100028b430006036e7332c00cc00c0002000100028b430006036e7333c00cc00c0002000100028b430006036e7334c00cc00c0002000100028b430006036e7331c00cc10e00010001000289e60004d8ef200ac0d800010001000289e60004d8ef220ac0ea00010001000289e60004d8ef240ac0fc00010001000289e60004d8ef260a"));
            
            

            if (test.IsPacketAnswer())
            {
                Console.WriteLine("Its answer");
                if (test.IsPacketSucces()) Console.WriteLine(test.getMinTTL());
            }
            else
            {
                Console.WriteLine("itsQuery");
            }

            if (test.IsPacketSucces())
            {
                Console.WriteLine("Its OK");
            }
            else
            {
                Console.WriteLine("its !OK");
            }
            
            Console.ReadLine();
        }
    }
}

