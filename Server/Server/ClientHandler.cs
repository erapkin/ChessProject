using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Server
{
    public class ClientHandler
    {

        TcpClient ClientSocket;
        bool ContinueProcess = false;
        Thread ClientThread;
        

        public ClientHandler(TcpClient ClientSocket)
        {
            this.ClientSocket = ClientSocket;
        }

        public void Start()
        {
            ContinueProcess = true;
            ClientThread = new Thread(new ThreadStart(Process));
            ClientThread.Start();
        }

        private void Process()
        {
            string dataFromClient = null;

            byte[] bytes;

            if (ClientSocket != null)
            {
                NetworkStream networkStream = ClientSocket.GetStream();
                ClientSocket.ReceiveTimeout = 100; //miliseconds

                while (ContinueProcess)
                {
                    bytes = new byte[ClientSocket.ReceiveBufferSize];
                    try
                    {
                        int BytesRead = networkStream.Read(bytes, 0, (int)ClientSocket.ReceiveBufferSize);
                        if (BytesRead > 0)
                        {
                            dataFromClient = Encoding.ASCII.GetString(bytes, 0, BytesRead);

                            foreach (TcpClient client in SynchronousSocketListener.ClientList)
                            {
                                if (client != this.ClientSocket)
                                {
                                    NetworkStream responseStream = client.GetStream();
                                    responseStream.Write(bytes, 0, bytes.Length);
                                }
                            }
                            Console.WriteLine("Text received : {0}", dataFromClient);
            
                           // byte[] sendBytes = Encoding.ASCII.GetBytes(dataFromClient);
                           // networkStream.Write(sendBytes, 0, sendBytes.Length);

                            if (dataFromClient == "quit") break;

                        }
                    }
                    catch (IOException) { } 
                    catch (SocketException)
                    {
                        Console.WriteLine("Conection is broken!");
                        break;
                    }
                    Thread.Sleep(200);
                } 
                networkStream.Close();
                ClientSocket.Close();
            }
        }  

        public void Stop()
        {
            ContinueProcess = false;
            if (ClientThread != null && ClientThread.IsAlive)
                ClientThread.Join();
        }

        public bool IsAlive
        {
            get
            {
                return (ClientThread != null && ClientThread.IsAlive);
            }
        }

    } 
}