using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Server
{
    public class SynchronousSocketListener
    {

        private const int portNum = 9999;
        private static ArrayList ClientSockets;
        private static bool ContinueReclaim = true;
        private static Thread ThreadReclaim;
        public static HashSet<TcpClient> ClientList = new HashSet<TcpClient>() { };

        public static void StartListening()
        {

            ClientSockets = new ArrayList();

            ThreadReclaim = new Thread(new ThreadStart(Reclaim));
            ThreadReclaim.Start();

            TcpListener listener = new TcpListener(IPAddress.Any, portNum);
            try
               {
                listener.Start();

                int Cycle = 3;
                int ClientNbr = 0;

                Console.WriteLine("Waiting for a connection...");
                while (Cycle > 0)
                {

                    TcpClient handler = listener.AcceptTcpClient();

                    if (handler != null)
                    {
                        Console.WriteLine("Client#{0} accepted!", ++ClientNbr);
                        
                        lock (ClientSockets.SyncRoot)
                        {
                            int i = ClientSockets.Add(new ClientHandler(handler));
                            ((ClientHandler)ClientSockets[i]).Start();

                            ClientList.Add(handler);

                        }
                        --Cycle;
                    }
                    else
                        break;
                }
                listener.Stop();

                ContinueReclaim = false;
                ThreadReclaim.Join();

                foreach (Object Client in ClientSockets)
                {
                    ((ClientHandler)Client).Stop();
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

            Console.WriteLine("\nHit enter to continue...");
            Console.Read();

        }

        private static void Reclaim()
        {
            while (ContinueReclaim)
            {
                lock (ClientSockets.SyncRoot)
                {
                    for (int x = ClientSockets.Count - 1; x >= 0; x--)
                    {
                        Object Client = ClientSockets[x];
                        if (!((ClientHandler)Client).IsAlive)
                        {
                            ClientSockets.Remove(Client);
                            Console.WriteLine("A client left");
                        }
                    }
                }
                Thread.Sleep(200);
            }
        }
    }
}