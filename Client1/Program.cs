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
    class Program
    {
        

        static void Main(string[] args)
        {   
            Client1 client1 = new Client1("Client1");
            client1.Connect();
            client1.LoopPacket();
            Client1.tcpclnt.Close();
        }

    }
}


