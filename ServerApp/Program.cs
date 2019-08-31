using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ServerApp
{
    class Program
    {
        static readonly object _lock = new object();
        static readonly Dictionary<int, TcpClient> list_clients = new Dictionary<int, TcpClient>();

        static void Main(string[] args)
        {
            Console.WriteLine("Welcome To the Hell");

            int count = 1;
            var port = Convert.ToInt32(ConfigurationManager.AppSettings.Get("ServerPort"));
            TcpListener ServerSocket = new TcpListener(IPAddress.Any, port);
            ServerSocket.Start();

            try
            {
                while (true)
                {
                    TcpClient client = ServerSocket.AcceptTcpClient();
                    lock (_lock) list_clients.Add(count, client);
                    Console.WriteLine($"{client.Client.RemoteEndPoint} Connected!");
                    Broadcast($"\t\t%%%%%{client.Client.RemoteEndPoint} Welcome To the Hell!");
                    Thread t = new Thread(Handle_clients);
                    t.Start(count);
                    count++;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        public static void Handle_clients(object o)
        {
            int id = (int)o;
            TcpClient client;
            lock (_lock) client = list_clients[id];
            var exitMsg = $"\t\t%%%%%{client.Client.RemoteEndPoint} Has Escaped The Hell!";

            try
            {
                while (true)
                {
                    NetworkStream stream = client.GetStream();
                    byte[] buffer = new byte[1024];
                    int byte_count = stream.Read(buffer, 0, buffer.Length);

                    if (byte_count == 0)
                    {
                        break;
                    }

                    string data = Encoding.ASCII.GetString(buffer, 0, byte_count);
                    Broadcast(data);
                    Console.WriteLine(data.Remove(0, 7));
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(exitMsg.Remove(0, 7));
            }

            lock (_lock) list_clients.Remove(id);
            client.Client.Shutdown(SocketShutdown.Both);
            client.Close();
            Broadcast(exitMsg);
        }

        public static void Broadcast(string data)
        {
            byte[] buffer = Encoding.ASCII.GetBytes(data + Environment.NewLine);

            lock (_lock)
            {
                foreach (TcpClient c in list_clients.Values)
                {
                    NetworkStream stream = c.GetStream();
                    stream.WriteAsync(buffer, 0, buffer.Length);
                }
            }
        }
    }
}