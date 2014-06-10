﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NTPServer
{
    class Program
    {
        static void Main(string[] args)
        {
            ThreadedServer ts = new ThreadedServer(8967);
            ts.Start();
            Console.ReadLine();
        }

        class ThreadedServer
        {
            private Socket _serverSocket;
            private int _port;

            public ThreadedServer(int port) { _port = port; }

            private class ConnectionInfo
            {
                public Socket Socket;
                public Thread Thread;
            }

            private Thread _acceptThread;
            private List<ConnectionInfo> _connections = new List<ConnectionInfo>();

            public void Start()
            {
                SetupServerSocket();
                _acceptThread = new Thread(AcceptConnections);
                _acceptThread.IsBackground = true;
                _acceptThread.Start();
            }

            private void SetupServerSocket()
            {
                // Получаем информацию о локальном компьютере
                IPHostEntry localMachineInfo =
                    Dns.GetHostEntry(Dns.GetHostName());
                IPEndPoint myEndpoint = new IPEndPoint(
                   localMachineInfo.AddressList[0], _port);

                // Создаем сокет, привязываем его к адресу
                // и начинаем прослушивание
                _serverSocket = new Socket(
                    myEndpoint.Address.AddressFamily,
                    SocketType.Stream, ProtocolType.Tcp);
                _serverSocket.Bind(myEndpoint);
                _serverSocket.Listen((int)
                    SocketOptionName.MaxConnections);
            }

            private void AcceptConnections()
            {
                while (true)
                {
                    // Принимаем соединение
                    Socket socket = _serverSocket.Accept();
                    ConnectionInfo connection = new ConnectionInfo();
                    connection.Socket = socket;

                    // Создаем поток для получения данных
                    connection.Thread = new Thread(ProcessConnection);
                    connection.Thread.IsBackground = true;
                    connection.Thread.Start(connection);

                    // Сохраняем сокет
                    lock (_connections) _connections.Add(connection);
                }
            }

            private void ProcessConnection(object state)
            {
                ConnectionInfo connection = (ConnectionInfo)state;
                byte[] buffer = new byte[255];
                try
                {
                    while (true)
                    {
                        int bytesRead = connection.Socket.Receive(
                            buffer);
                        if (bytesRead > 0)
                        {
                            lock (_connections)
                            {
                                foreach (ConnectionInfo conn in
                                    _connections)
                                {
//                                    if (conn != connection)
//                                    {
//                                        conn.Socket.Send(
//                                            buffer, bytesRead,
//                                            SocketFlags.None);
//                                    }
                                    Console.WriteLine(buffer);
                                }
                            }
                        }
                        else if (bytesRead == 0) return;
                    }
                }
                catch (SocketException exc)
                {
                    Console.WriteLine("Socket exception: " +
                        exc.SocketErrorCode);
                }
                catch (Exception exc)
                {
                    Console.WriteLine("Exception: " + exc);
                }
                finally
                {
                    connection.Socket.Close();
                    lock (_connections) _connections.Remove(
                        connection);
                }
            }
        }
    }
}
