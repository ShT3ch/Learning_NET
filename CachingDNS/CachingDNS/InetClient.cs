using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;namespace CachingDNS
{
    class InetClient
    {
        private const int ListenPort = 53;
        private string TrueDNS = "8.8.8.8";
        public delegate byte[] DNSHandler(byte[] packet);

        private Cacher Handler;

        public InetClient(Cacher handler)
        {
            Handler = handler;
            handler.Nc = this;
        }

        public void StartListener()
        {
            bool done = false;

            var listener = new UdpClient(ListenPort);
            var groupEP = new IPEndPoint(IPAddress.Any, ListenPort);
            try
            {


                while (!done)
                {
                    try
                    {
                        groupEP = new IPEndPoint(IPAddress.Any, ListenPort);
                        
                        Console.WriteLine("ClientWaiting");
                        byte[] bytes = new byte[100];
                        while (listener.Available < 1)
                        {
                            Thread.Sleep(5);
                        }

                        try
                        {
                            bytes = listener.Receive(ref groupEP);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("Recieving error");
                            throw;
                        }

                        var answer = Handler.GetAnswer(bytes);
                        try
                        {
                            listener.Send(answer, answer.Length, groupEP);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("Sending error");
                            throw;
                        }
                        Console.WriteLine("Client from {0} handled", groupEP);
                    }
                    catch (Exception e)
                    {
                        listener.Close();
                        listener = new UdpClient(ListenPort);
                    }

                }
            }
            finally
            {
                listener.Close();
            }
        }

    

        public byte[] makeQuery(byte[] query)
        {
//            byte[] answer;
            var groupEP = new IPEndPoint(IPAddress.Parse(TrueDNS),port:ListenPort);

            var sender = new UdpClient(TrueDNS,ListenPort);
            byte[] ans=new byte[1];
            
            try
            {
                sender.Send(query, query.Length);
                ans = sender.Receive(ref groupEP);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.ReadLine();
            }
            finally
            {
                sender.Close();
            }


            return ans;
        }
    }
}
