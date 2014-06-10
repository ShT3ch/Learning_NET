using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace NTPS3rv3r
{
    public struct NTP_packet
    {
        public byte[] header; //IK(2) HB(3) mode(3)
        public byte[] strate;
        public byte[] interval;
        public byte[] accuracy;
        public byte[] latency;
        public byte[] variance;
        public byte[] sourceID;
        public byte[] timeRefresh;
        public byte[] timebegin;
        public byte[] timeRecieve;
        public byte[] timeSend;
        public byte[] IDKey;
        public byte[] Daigest;

        public UInt32 timeSend_int;
        //        private string timeSend_str;

        public NTP_packet(byte[] packet)
        {
            try
            {
                header = new byte[1];
                Array.Copy(packet, 0, header, 0, 1);
                header = header.Reverse().ToArray();

                strate = new byte[1];
                Array.Copy(packet, 1, strate, 0, 1);

                interval = new byte[1];
                Array.Copy(packet, 2, interval, 0, 1);

                accuracy = new byte[1];
                Array.Copy(packet, 3, accuracy, 0, 1);

                latency = new byte[4];
                Array.Copy(packet, 4, latency, 0, 4);

                variance = new byte[4];
                Array.Copy(packet, 8, variance, 0, 4);

                sourceID = new byte[4];
                Array.Copy(packet, 12, sourceID, 0, 4);

                timeRefresh = new byte[8];
                Array.Copy(packet, 16, timeRefresh, 0, 8);

                timebegin = new byte[8];
                Array.Copy(packet, 24, timebegin, 0, 8);

                timeRecieve = new byte[8];
                Array.Copy(packet, 32, timeRecieve, 0, 8);

                timeSend = new byte[8];
                Array.Copy(packet, 40, timeSend, 0, 8);
                if (BitConverter.IsLittleEndian) timeSend = timeSend.Reverse().ToArray();
                timeSend_int = BitConverter.ToUInt32(timeSend, 4);

                IDKey = new byte[4];
                //            Array.Copy(packet, 48, variance, 0, 4);

                Daigest = new byte[16];
                //            Array.Copy(packet, 52, Daigest, 0, 16);

                //            timeSend_int = 0;
            }
            finally
            {
            }
        }

        //       
        public NTP_packet(DateTime begin, NTP_packet donor, TimeSpan Error)
        {
            header = donor.header;
            header[0]++;

            strate = donor.strate;

            interval = donor.interval;

            accuracy = donor.accuracy;

            latency = donor.latency;

            variance = donor.variance;

            sourceID = donor.sourceID;

            timeRefresh = donor.timeRefresh;

            timebegin = donor.timeSend.Reverse().ToArray();

            timeRecieve = new byte[8];
            timeRecieve = BitConverter.GetBytes(((UInt64)(begin - new DateTime(1900, 1, 1)+ Error).TotalSeconds) << 32).Reverse().ToArray();

            timeSend = new byte[8];

            IDKey = new byte[4] {0, 0, 0, 0};
            //            Array.Copy(packet, 48, variance, 0, 4);

            Daigest = new byte[16] {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0};
            //            Array.Copy(packet, 52, Daigest, 0, 16);

            //            timeSend_int = 0;
            timeSend_int = 0;
        }

        public void setSendTime(TimeSpan Error)
        {
            timeSend = BitConverter.GetBytes(((UInt64)(DateTime.UtcNow - new DateTime(1900, 1, 1)+Error).TotalSeconds) << 32).Reverse().ToArray();
        }

        public byte[] serialize()
        {
            var ans = new byte[68];

            Array.Copy(header, 0, ans, 0, 1);
            Array.Copy(strate, 0, ans, 1, 1);
            Array.Copy(interval, 0, ans, 2, 1);
            Array.Copy(accuracy, 0, ans, 3, 1);
            Array.Copy(latency, 0, ans, 4, 4);
            Array.Copy(variance, 0, ans, 8, 4);
            Array.Copy(sourceID, 0, ans, 12, 4);
            Array.Copy(timeRefresh, 0, ans, 16, 8);
            Array.Copy(timebegin, 0, ans, 24, 8);
            Array.Copy(timeRecieve, 0, ans, 32, 8);
            Array.Copy(timeSend, 0, ans, 40, 8);
            Array.Copy(IDKey, 0, ans, 48, 4);
            Array.Copy(Daigest, 0, ans, 52, 16);

            return ans;
        }
    }


    public class UDPListener
    {
        private const int listenPort = 123;

        public UDPListener(int Error_minute)
        {
            bool done = false;
            UdpClient listener = new UdpClient(listenPort);
            IPEndPoint groupEP = new IPEndPoint(IPAddress.Any, listenPort);
            string received_data;
            string hex_data;
            byte[] receive_byte_array;
            var timeEditor = new DateTime();

            try
            {
                while (!done)
                {
                    Console.WriteLine("Waiting for incoming");

                    receive_byte_array = listener.Receive(ref groupEP);
                    Console.WriteLine("Received a broadcast from {0}", groupEP);

                    received_data = Encoding.ASCII.GetString(receive_byte_array, 0, receive_byte_array.Length);

                    var recieve_time = DateTime.UtcNow;

                    hex_data = BitConverter.ToString(receive_byte_array);
                    try
                    {
                        var strange = new NTP_packet(receive_byte_array);

                        var ans = new NTP_packet(recieve_time, strange, new TimeSpan(0, Error_minute, 0));
                        ans.setSendTime(new TimeSpan(0, Error_minute, 0));

                        var answer = ans.serialize();

                        var server = new UdpClient(groupEP.Address.ToString(), groupEP.Port);

                        server.Send(answer, 68);

                        Console.WriteLine("byte as Hex:" + hex_data.PadRight(30));
                        Console.WriteLine("data follows \n{0}\n\n", received_data);
                        Console.WriteLine("Date: {0}\n\n", strange.timeSend_int);
                    }
                    catch (Exception e)
                    {
                        System.Console.WriteLine("something wrong with recieved packet");
                    }
                }
            }

            finally
            {
                listener.Close();
            }
        }
    }

    internal class Program
    {
        private static void Main(string[] args)
        {
            if (args.Count() == 1)
            {
                new UDPListener(int.Parse(args[0]));
            }
            else
            {
                Console.WriteLine("something wrong with arguments");
            }
        }
    }
}
