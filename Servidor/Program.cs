﻿
using System;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;

namespace servidorAsincrono
{

    public class AsynchronousSocketListener
    {
        public static Dictionary<string, string> archivos = new Dictionary<string, string>();

        public static RSAParameters clavePublica; //Aqui guardaremos la clave publica del cliente

        public static int TAM = 1024;

        // Incoming data from the client.  
        public static string data;

        public static async Task StartListening()
        {
            //Crear archivos en el servidor
            for (int i = 1; i <= 10; i++)
            {
                string archivo = "archivo" + i + ".txt";
                string contenidoArc = "Archivo" + i;
                File.WriteAllText(archivo, contenidoArc);
                archivos.Add(archivo,contenidoArc);
            }
            
            // Data buffer for incoming data.  
            byte[] bytes = new Byte[TAM];

            // Establish the local endpoint for the socket.  
            // Dns.GetHostName returns the name of the   
            // host running the application.  
            //IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
            //IPAddress ipAddress = ipHostInfo.AddressList[0];
            IPAddress ipAddress = getLocalIpAddress();//MAC OS
            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, 11000);

            // Create a TCP/IP socket.  
            Socket listener = new Socket(ipAddress.AddressFamily,
                SocketType.Stream, ProtocolType.Tcp);

            // Bind the socket to the local endpoint and   
            // listen for incoming connections.  
            try
            {
                listener.Bind(localEndPoint);
                listener.Listen(10);

                // Start listening for connections.  
                while (true)
                {
                    Console.WriteLine("Waiting for a connection...");
                    // Program is suspended while waiting for an incoming connection.  
                    Socket handler = await listener.AcceptAsync();
                     data = null;
                    int bytesRec = await handler.ReceiveAsync(new ArraySegment<byte>(bytes), SocketFlags.None);
                    data += Encoding.ASCII.GetString(bytes, 0, bytesRec);
                    // An incoming connection needs to be processed.  
                    while (bytesRec == TAM)
                    {
                        bytesRec = await handler.ReceiveAsync(new ArraySegment<byte>(bytes), SocketFlags.None);
                        data += Encoding.ASCII.GetString(bytes, 0, bytesRec);
                    }
                    Console.WriteLine("Json:");
                    Console.WriteLine(data); // De momento solo mostrar la clave para asegurar que esta llegando bien.
                  
                    // Echo the data back to the client.  
                    
                    byte[] msg = Encoding.ASCII.GetBytes(data.ToString());
                    Console.WriteLine(msg);
                    await handler.SendAsync(new ArraySegment<byte>(msg), SocketFlags.None);
                    handler.Shutdown(SocketShutdown.Both);
                    handler.Close();
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

            Console.WriteLine("\nPress ENTER to continue...");
            Console.Read();

        }

        static IPAddress getLocalIpAddress()
        {
            IPAddress ipAddress = null;
            try
            {
                foreach (var netInterface in NetworkInterface.GetAllNetworkInterfaces())
                {
                    if (netInterface.NetworkInterfaceType == NetworkInterfaceType.Wireless80211 ||
                        netInterface.NetworkInterfaceType == NetworkInterfaceType.Ethernet)
                    {
                        foreach (var addrInfo in netInterface.GetIPProperties().UnicastAddresses)
                        {
                            if (addrInfo.Address.AddressFamily == AddressFamily.InterNetwork)
                            {
                                ipAddress = addrInfo.Address;
                            }
                        }
                    }
                }
                if (ipAddress == null)
                {
                    IPHostEntry ipHostInfo = Dns.GetHostEntry("127.0.0.1");
                    ipAddress = ipHostInfo.AddressList[0];
                }
            }
            catch (Exception) { }
            return ipAddress;
        }

        public static async Task Main(String[] args)
        {
            await StartListening();
        }
    }
}
// > dotnet run
// Waiting for a connection...
// Text received : This is a test
// Waiting for a connection...
// 