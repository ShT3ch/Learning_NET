using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HTTP_Proxy
{

    class InetHandler
    {

        private TcpListener Listener;
        private Thread ListenerThread;
        private int Port = 666;

        public InetHandler(int port)
        {
            Listener = new TcpListener(new IPEndPoint(IPAddress.Any, Port));
            ListenerThread = new Thread(HandleLoop);
        }

        public void startHandling()
        {
            ListenerThread.Start();
        }

        private void HandleLoop()
        {
            Listener.Start();
            while (true)
            {
                Listener.BeginAcceptTcpClient(SomebodyConnected, new object());
                Thread.Sleep(1);
            }
        }

        private void SomebodyConnected(Object obj)
        {
            var client = Listener.EndAcceptTcpClient((System.IAsyncResult)obj);

            (new Thread(ClientService)).Start(client);
        }

        private void ClientService(Object Client)
        {
            var client = (TcpClient)Client;

            var filename = System.IO.Path.GetRandomFileName();

            Directory.CreateDirectory("resourses");

            var file = File.Create(Path.Combine("resourses", filename));

            try
            {
                var stream = client.GetStream();

                var buffer = new byte[2048000];

                while (stream.DataAvailable)
                {
                    var toRead = Math.Min(buffer.Length, client.Available);
                    stream.Read(buffer, 0, toRead);
                    Thread.Sleep(50);
                    file.Write(buffer,0,toRead);
                }
                file.Close();
                client.Close();
            }
            catch (Exception e)
            {

            }
            finally
            {
                if (client.Connected) client.Close();
            }
        }
    }
}
