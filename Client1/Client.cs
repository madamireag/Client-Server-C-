using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Client1
{
    public class Client
    {
        public static TcpClient tcpclnt = new TcpClient();
        public static string Name { get; set; }

        public void Connect()
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

        private static void ReadMessageFromConsoleAndWriteInFile(string filePath)
        {
            using (StreamWriter writer = new StreamWriter(filePath, true))
            {
                // citesc de la consola ce vreau sa scriu in fisier
                Console.WriteLine("Write a message: ");
                string input = Console.ReadLine();

                //scriu in fisier
                int.TryParse(input, out int val);

                writer.WriteLine(Name + " " + input);

                //inchid fisierul/stream-ul
                writer.Close();
            }
        }

        private static bool IsFileLocked(string filePath)
        {
            try
            {
                FileStream stream = File.Open(filePath, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
                stream.Close();
            }
            catch (IOException)
            {
                return true;
            }

            return false;
        }

        private static void ReadFromFile(string filePath)
        {
            string[] linesArr =
                           File.ReadAllLines(filePath);
            List<string> listOfLines = new List<string>();
            listOfLines.AddRange(linesArr);
            for (int i = 0; i < listOfLines.Count; i++)
            {
                string[] wholeMessage = listOfLines[i].Split(" ", StringSplitOptions.None);
                string recipient = wholeMessage[wholeMessage.Length - 1];
                // if the message is for me
                if (recipient.ToLower().Equals(Name.ToLower()))
                {
                    //find out who's it from
                    string sender = wholeMessage[0];

                    // get only the message (without first word - sender and last word - recipient)
                    string actualMessage = listOfLines[i].Substring(sender.Length + 1, listOfLines[i].Length - (sender.Length + 1) - (recipient.Length + 1));

                    // write what i received
                    Console.WriteLine($"Message received from {sender}: {actualMessage}");

                    // delete the message cause i already processed it
                    listOfLines.Remove(listOfLines[i]);

                    //write the confirmation message in the file
                    if (actualMessage.ToLower() != "message received and processed!")
                        listOfLines.Add(Name + " Message received and processed! " + sender);
                }
            }
            File.WriteAllLines(filePath, listOfLines.ToArray());
        }

        public void LoopPacket()
        {
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
                        ReadMessageFromConsoleAndWriteInFile(filePath);

                        //de adaugat citirea ciclica din fisier a mesajelor pentru mine + trimitere confirmare mesaje primite
                        new Thread(() =>
                        {
                            Thread.CurrentThread.IsBackground = true;
                            ReadFromFile(filePath);
                        }).Start();

                    }
                    else if (IsFileLocked(filePath))
                    {
                        Console.WriteLine($"The file is locked! Retrying in {timeout / 1000} seconds");
                    }
                    // sleep for the configured timeout
                    Thread.Sleep(timeout);
                }
                catch (Exception ex)
                {

                    Console.WriteLine("Error: " + ex.Message);
                    break;
                }

            }
        }

    }

}
