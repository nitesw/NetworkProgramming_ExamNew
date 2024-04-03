using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ChatServer
{
    class Program
    {
        static void Main(string[] args)
        {
            string remoteIP = "192.168.1.104";
            short remotePort = 8080;

            IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Parse(remoteIP), remotePort);
            IPEndPoint clientEndPoint = null;

            UdpClient server = new UdpClient(localEndPoint);

            HashSet<IPEndPoint> connectedUsers = new HashSet<IPEndPoint>();

            while (true)
            {
                try
                {
                    Console.WriteLine("Waiting for a message...");
                    byte[] receivedData = server.Receive(ref clientEndPoint);
                    string message = Encoding.Unicode.GetString(receivedData);

                    Console.WriteLine($"Got the {message} message from {clientEndPoint}");
                    switch (message)
                    {
                        case "<JOIN>":
                            connectedUsers.Add(clientEndPoint);
                            break;
                        case "<LEAVE>":
                            connectedUsers.Remove(clientEndPoint);
                            break;
                        default:
                            foreach (var user in connectedUsers)
                            {
                                server.Send(receivedData, receivedData.Length, user);
                            }
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }
    }
}
