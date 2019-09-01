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
using System.Windows.Forms;

namespace ClientApp
{
    class Program
    {
        static string name = ConfigurationManager.AppSettings.Get("Name");
        static readonly string ip = ConfigurationManager.AppSettings.Get("ServerIP");
        static readonly int port = Convert.ToInt32(ConfigurationManager.AppSettings.Get("ServerPort"));
        static string colour = ConfigurationManager.AppSettings.Get("Colour");
        static StringBuilder sb = new StringBuilder();
        static Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
        static string s;

        static void Main(string[] args)
        {

            try
            {
                TcpClient client = new TcpClient();
                client.Connect(ip, port);
                Console.WriteLine("Connected to the server!");

                NetworkStream ns = client.GetStream();
                Thread thread = new Thread(async o =>  await ReceiveData((TcpClient)o));
                thread.Start(client);

                while (true)
                {
                    s = Console.ReadLine();
                    DeletePrevConsoleLine();
                    HandleUserCommands(s, ns);
                    sb.Clear();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.ReadKey();
            }
        }

        static async Task ReceiveData(TcpClient client)
        {
            NetworkStream ns = client.GetStream();
            byte[] receivedBytes = new byte[1024];
            int byte_count;

            while ((byte_count = await ns.ReadAsync(receivedBytes, 0, receivedBytes.Length)) > 0)
            {
                var dataReceived = Encoding.ASCII.GetString(receivedBytes, 0, byte_count);
                var remoteUserColour = dataReceived.Substring(0,7);
                Console.ForegroundColor = StringToConsoleColorConverter(remoteUserColour);

                try
                {
                    if (s.Length > 0)
                    {
                        Console.WriteLine();
                        DeletePrevConsoleLine();
                    }
                }
                catch (Exception)
                {}

                Console.Write(dataReceived.Remove(0,7));
                Console.ForegroundColor = StringToConsoleColorConverter(colour);
            }
        }

        static void HandleUserCommands(string data, NetworkStream ns)
        {
            string userInput;

            try
            {
                userInput = data.Trim().Substring(0, 2);
            }
            catch (Exception)
            {
                userInput = data;
            }

            switch (userInput)
            {
                case "-c":
                    {
                        colour = data.Substring(2, data.Length - 2).Trim();
                        Console.ForegroundColor = StringToConsoleColorConverter(colour);
                        Console.WriteLine($"Temporarily Changed the Colour to : {colour}");
                    }
                    break;
                case "-C":
                    {
                        colour = data.Substring(2, data.Length - 2).Trim();
                        Console.ForegroundColor = StringToConsoleColorConverter(colour);
                        config.AppSettings.Settings.Remove("Colour");
                        config.AppSettings.Settings.Add("Colour", colour);
                        config.Save(ConfigurationSaveMode.Minimal);
                        Console.WriteLine($"Permanently Changed the Colour to : {colour}");
                    }
                    break;
                case "-r":
                    {
                        var nameTemp = name;
                        name = data.Substring(2, data.Length - 2).Trim();
                        sb.Append($"{ColourToStringConverter(colour)}{nameTemp} Temporarily Changed the Name to {name}");
                        byte[] buffer = Encoding.ASCII.GetBytes(sb.ToString());
                        ns.Write(buffer, 0, buffer.Length);
                    }
                    break;
                case "-R":
                    {
                        var nameTemp = name;
                        name = data.Substring(2, data.Length - 2).Trim();
                        config.AppSettings.Settings.Remove("Name");
                        config.AppSettings.Settings.Add("Name", name);
                        config.Save(ConfigurationSaveMode.Full);
                        sb.Append($"{ColourToStringConverter(colour)}{nameTemp} Permanently Changed the Name to {name}");
                        byte[] buffer = Encoding.ASCII.GetBytes(sb.ToString());
                        ns.Write(buffer, 0, buffer.Length);
                    }
                    break;
                case "-h":
                case "-H":
                    {
                        Console.WriteLine("Welcome to Help");
                        Console.WriteLine(" -c Temporarily Change the user text colour");
                        Console.WriteLine(" -C Permanently Change the user text colour");
                        Console.WriteLine("    Available Text Colours : Red, White, Yellow, Magenta, Cyan, Green, Blue, Gray, DarkGray, DarkMagenta, DarkRed, DarkCyan, DarkGreen, DarkBlue ");
                        Console.WriteLine(" -r Temporarily Change the username");
                        Console.WriteLine(" -R Permanently Change the username");
                        Console.WriteLine(" -X Close the Client");
                    }
                    break;
                case "-x":
                    {
                        Environment.Exit(0);
                    }
                    break;
                default:
                    {
                        sb.Append($"{ColourToStringConverter(colour)}{name}: {data}");
                        byte[] buffer = Encoding.ASCII.GetBytes(sb.ToString());
                        ns.Write(buffer, 0, buffer.Length);
                    }
                    break;
            }
        }

        private static void DeletePrevConsoleLine()
        {
            var lineCount = (s.Length / Console.WindowWidth) + 1;

            if (Console.CursorTop == 0) return;

            Console.SetCursorPosition(0, Console.CursorTop - lineCount);

            for (int i = 0; i <= lineCount; i++)
            {
                Console.Write(new string(' ', Console.WindowWidth));
            }

            Console.SetCursorPosition(0, Console.CursorTop - (lineCount -1));
        }


        private static ConsoleColor StringToConsoleColorConverter(string color)
        {
            ConsoleColor consoleColor = ConsoleColor.White;

            switch (color)
            {
                case "xFF0000":
                    consoleColor = ConsoleColor.Red;
                    break;
                case "xFFFFFF":
                    consoleColor = ConsoleColor.White;
                    break;
                case "xFFFF00":
                    consoleColor = ConsoleColor.Yellow;
                    break;
                case "xFF00FF":
                    consoleColor = ConsoleColor.Magenta;
                    break;
                case "x00FFFF":
                    consoleColor = ConsoleColor.Cyan;
                    break;
                case "x008000":
                    consoleColor = ConsoleColor.Green;
                    break;
                case "x0000FF":
                    consoleColor = ConsoleColor.Blue;
                    break;
                case "x808080":
                    consoleColor = ConsoleColor.Gray;
                    break;
                case "xA9A9A9":
                    consoleColor = ConsoleColor.DarkGray;
                    break;
                case "x8B008B":
                    consoleColor = ConsoleColor.DarkMagenta;
                    break;
                case "x8B0000":
                    consoleColor = ConsoleColor.DarkRed;
                    break;
                case "x008B8B":
                    consoleColor = ConsoleColor.DarkCyan;
                    break;
                case "x006400":
                    consoleColor = ConsoleColor.DarkGreen;
                    break;
                case "x00008B":
                    consoleColor = ConsoleColor.DarkBlue;
                    break;
                default:
                    break;
            }

            return consoleColor;
        }

        private static string ColourToStringConverter(string colour)
        {
            string userColour;

            switch (colour)
            {
                case "Red":
                    userColour = "xFF0000";
                    break;
                case "White":
                    userColour = "xFFFFFF";
                    break;
                case "Yellow":
                    userColour = "xFFFF00";
                    break;
                case "Magenta":
                    userColour = "xFF00FF";
                    break;
                case "Cyan":
                    userColour = "x00FFFF";
                    break;
                case "Green":
                    userColour = "x008000";
                    break;
                case "Blue":
                    userColour = "x0000FF";
                    break;
                case "Gray":
                    userColour = "x808080";
                    break;
                case "DarkGray":
                    userColour = "xA9A9A9";
                    break;
                case "DarkMagenta":
                    userColour = "x8B008B";
                    break;
                case "DarkRed":
                    userColour = "x8B0000";
                    break;
                case "DarkCyan":
                    userColour = "x008B8B";
                    break;
                case "DarkGreen":
                    userColour = "x006400";
                    break;
                case "DarkBlue":
                    userColour = "x00008B";
                    break;
                default:
                    userColour = "xFFFFFF";
                    break;
            }

            return userColour;
        }
    }
}