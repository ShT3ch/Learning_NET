using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MiniMailerWPF
{
    internal class InetOperator
    {
        private readonly TcpClient NetHandler;

        public MainWindow.Alarm Alarmer;


        public InetOperator(string adress, int port, MainWindow.Alarm alyarm)
        {
            Alarmer = alyarm;

            try
            {
                NetHandler = new TcpClient(adress, port);
            }
            catch (Exception e)
            {
                Alarmer(e.Message);
            }
        }

        public bool isConnected()
        {
            return NetHandler.Connected;
        }

        public string Recieve()
        {
            var reader = new NetworkStream(NetHandler.Client);

            var counter = DateTime.Now;
            while (!reader.DataAvailable)
            {
                Thread.Sleep(5);
                if ((counter - DateTime.Now).Milliseconds > 500)
                {
                    throw new TimeoutException("Too long waiting in recieve.");
                }
            }

            var response = "";
            while (reader.DataAvailable)
            {
                var buffer = new byte[100];
                reader.Read(buffer, 0, 100);
                response += Encoding.UTF8.GetString(buffer);
            }

            response=response.Split('\0')[0];

            Console.Write(response);

            return response;
        }

        public void Send(string data)
        {
            Console.WriteLine(data);
            var writer = new StreamWriter(NetHandler.GetStream());
            NetHandler.GetStream().Write(Encoding.ASCII.GetBytes(data + "\r\n"), 0, Encoding.ASCII.GetBytes(data + "\r\n").Length);
            writer.Flush();
        }

        public string SendRecieve(string data)
        {
            lock (this)
            {
                try
                {
                    Send(data);

                    var ans = Recieve();

                    return ans;
                }
                catch (Exception e)
                {
                    Alarmer(e.Message);
                }
                return null;
            }
        }

        static byte[] GetBytes(string str)
        {
            byte[] bytes = new byte[str.Length * sizeof(char)];
            System.Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
            return bytes;
        }

        static string GetString(byte[] bytes)
        {
            char[] chars = new char[bytes.Length / sizeof(char)];
            System.Buffer.BlockCopy(bytes, 0, chars, 0, bytes.Length);
            return new string(chars);
        }

    }
}
