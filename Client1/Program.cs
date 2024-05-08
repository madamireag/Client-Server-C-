using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Client1
{
    class Program
    {
        public static TcpClient tcpclnt = new TcpClient();

        static void Main(string[] args)
        {
            Connect();
            LoopPacket();
            tcpclnt.Close();
        }

        private static bool IsFileLocked(string filePath)
        {
            FileStream stream = null;
            try
            {
                stream = File.Open(filePath, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
                stream.Close();
            }
            catch (IOException)
            {
                return true;
            }

            return false;
        }
        private static void LoopPacket()
        {
            byte[] bytesFrom = new byte[4096];
            string dataFromServer;
            var builder = new ConfigurationBuilder();
            builder.SetBasePath(Directory.GetCurrentDirectory())
                   .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
            IConfiguration config = builder.Build();

            while (true)
            {
                try
                {
                    string filePath = config["filePath"];
                    int.TryParse(config["clientTimeout"], out int timeout);

                    if (File.Exists(filePath) && !IsFileLocked(filePath))
                    {
                        using (StreamWriter writer = new StreamWriter(filePath, true))
                        {
                            // citesc de la consola ce vreau sa scriu in fisier
                            Console.WriteLine("Write a message:");
                            string input = Console.ReadLine();

                            //scriu in fisier
                            //if(int.Parse(input) == 0)
                            //    break;
                            writer.WriteLine(input);

                            //inchid fisierul/stream-ul
                            writer.Close();
                        }

                        // de adaugat citirea ciclica din fisier - mai incolo
                        // cum stiu ca e mesajul pt mine? - convin ca ultimul cuvant e destinatarul

                        NetworkStream networkStream = tcpclnt.GetStream();
                        networkStream.Read(bytesFrom, 0, 4096);
                        dataFromServer = Encoding.ASCII.GetString(bytesFrom);
                        dataFromServer = dataFromServer.Substring(0, dataFromServer.IndexOf("\0"));
                        Console.WriteLine("Server: " + dataFromServer);
                        networkStream.Flush();


                    }
                    else if (IsFileLocked(filePath))
                    {
                        Console.WriteLine($"The file is locked! Retrying in {timeout / 60000} min");
                    }

                    Thread.Sleep(timeout);
                }
                catch (Exception ex)
                {

                    Console.WriteLine("Error: " + ex.Message);
                    break;
                }

            }
        }

        private static void Connect()
        {
            Console.WriteLine("Connecting...");
            try
            {
                tcpclnt.Connect(IPAddress.Parse("127.0.0.1"), 9999);
                if (tcpclnt.Connected)
                {
                    Console.WriteLine("Connected!");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error connecting: " + ex.Message);
            }
        }
    }
}


