using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ClientApp
{
    class Program
    {
        static void Main(string[] args)
        {
            

            try
            {
                var name = ConfigurationManager.AppSettings.Get("Name");

                int port = 9999;
                TcpClient client = new TcpClient();
                client.Connect("localhost", port);
                Console.WriteLine("Connected to the server!");
                NetworkStream ns = client.GetStream();
                Thread thread = new Thread(o => ReceiveData((TcpClient)o));

                thread.Start(client);

                StringBuilder sb = new StringBuilder();
                string s;

                while (!string.IsNullOrEmpty((s = Console.ReadLine())))
                {
                    DeletePrevConsoleLine();
                    sb.Append($"{name}: {s}");
                    byte[] buffer = Encoding.ASCII.GetBytes(sb.ToString());
                    ns.Write(buffer, 0, buffer.Length);
                    sb.Clear();
                }

                client.Client.Shutdown(SocketShutdown.Send);
                thread.Join();

                ns.Close();
                client.Close();

                Console.WriteLine("disconnected from server!!");
                Console.ReadKey();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.ReadKey();
            }
        }

        static void ReceiveData(TcpClient client)
        {
            NetworkStream ns = client.GetStream();
            byte[] receivedBytes = new byte[1024];
            int byte_count;

            while ((byte_count = ns.Read(receivedBytes, 0, receivedBytes.Length)) > 0)
            {
                Console.Write(Encoding.ASCII.GetString(receivedBytes, 0, byte_count));
            }
        }

        private static void DeletePrevConsoleLine()
        {
            if (Console.CursorTop == 0) return;
            Console.SetCursorPosition(0, Console.CursorTop - 1);
            Console.Write(new string(' ', Console.WindowWidth));
            Console.SetCursorPosition(0, Console.CursorTop - 1);
        }
    }
}