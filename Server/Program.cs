
using Microsoft.Extensions.Configuration;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Server
{
   class Program
    {

        static void Main(string[] args)
        {
            Program main = new Program();
            main.server_start();

            Console.ReadLine();
        }

        TcpListener server = new TcpListener(IPAddress.Parse("127.0.0.1"), 9999);

        private void server_start()
        {
            server.Start();
            Console.WriteLine("Server Started");
            server.BeginAcceptTcpClient(handle_connection, server);
        }

        private static bool IsFileLocked(string filePath)
        {
            FileStream stream = null;
            try
            {
                stream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.None);
            }
            catch (IOException)
            {
                return true;
            }

            if (stream != null)
                stream.Close();

            return false;
        }

        private void handle_connection(IAsyncResult result)  
        {
            server.BeginAcceptTcpClient(handle_connection, server);
            TcpClient client = server.EndAcceptTcpClient(result);  

            string clientIP = ((IPEndPoint)client.Client.RemoteEndPoint).Address.ToString();
            int clientPort = ((IPEndPoint)client.Client.RemoteEndPoint).Port;
            Console.WriteLine(clientIP +":"+ clientPort+" connected");

            var builder = new ConfigurationBuilder();
            builder.SetBasePath(Directory.GetCurrentDirectory())
                   .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

            IConfiguration config = builder.Build();

            string filePath = config["filePath"];
            int.TryParse(config["serverTimeout"], out int timeout);


            // de adaugat aici citit la ciclul configurat
            while (true)
            {
                try
                {
                    if (File.Exists(filePath) && !IsFileLocked(filePath))
                    {
                        string[] linesArr =
                            File.ReadAllLines(filePath);
                        List<string> listOfLines = new List<string>();
                        listOfLines.AddRange(linesArr);
                        List<string> remainingLines = new List<string>();

                        //sterg mesajele procesate ca sa nu le citesc de 2 ori si trimit confirmarea clientului
                        foreach (string l in listOfLines)
                        {
                            string[] message = l.Split(" ", StringSplitOptions.None);
                            if (message[message.Length - 1].ToLower().Equals("server"))
                            {
                                // ii trimit confirmare clientului 
                                NetworkStream ns = client.GetStream();
                                byte[] messageInBytes = new byte[4096];
                                messageInBytes = Encoding.ASCII.GetBytes("Message received and processed!");
                                ns.Write(messageInBytes, 0, messageInBytes.Length);
                                //scriu ce mesaje am primit
                                Console.WriteLine($"Message received: {l}");
                            }
                            else { remainingLines.Add(l); }
                        }

                        File.WriteAllLines(filePath, remainingLines.ToArray());

                    }
                    else if (IsFileLocked(filePath))
                    {
                        Console.WriteLine($"The file is locked! Retrying in {timeout / 60000} min");

                    }
                    Thread.Sleep(timeout);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }

            }
        }
    }
}
