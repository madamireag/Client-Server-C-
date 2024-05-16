using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Client2
{
    class Program
    {
        public static TcpClient tcpclnt = new TcpClient();

        static void Main(string[] args)
        {
            Client2 client2 = new Client2("Client2");
            client2.Connect();
            client2.LoopPacket();
            Client2.tcpclnt.Close();
        }

    }
}
