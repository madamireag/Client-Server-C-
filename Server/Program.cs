
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
                stream = File.Open(filePath, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
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

                        //sterg mesajele procesate ca sa nu le citesc de 2 ori si trimit confirmarea clientului
                        for ( int i = 0; i < listOfLines.Count;i++)
                        {
                            string[] wholeMessage = listOfLines[i].Split(" ", StringSplitOptions.None);
                            string recipient = wholeMessage[wholeMessage.Length - 1];
                            if (recipient.ToLower().Equals("server"))
                            {
                                //find out who sent me the message
                                string sender = wholeMessage[0];

                                string actualMessage = listOfLines[i].Substring(sender.Length + 1, listOfLines[i].Length - (sender.Length + 1) - (recipient.Length + 1));

                                //scriu ce mesaje am primit
                                Console.WriteLine($"Message received from {sender}: {actualMessage}");

                                // delete the message cause i already processed it
                                listOfLines.Remove(listOfLines[i]);
                                //write the confirmation message in the file
                                if (actualMessage.ToLower() != "message received and processed!")
                                    listOfLines.Add("Server " + "Message received and processed! " + sender);
                            }
                        }

                        File.WriteAllLines(filePath, listOfLines.ToArray());
                    }
                    else if (IsFileLocked(filePath))
                    {
                        Console.WriteLine($"The file is locked! Retrying in {timeout / 1000} seconds");

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
