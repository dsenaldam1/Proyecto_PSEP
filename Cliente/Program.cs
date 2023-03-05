using System;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Security.Cryptography;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;

namespace clientesincrono {
    public class SynchronousSocketClient {

        public static void StartClient() {

            RSACryptoServiceProvider RSAalg = new RSACryptoServiceProvider(1024); //creamos el RSA en el cliente
            string publicKey = RSAalg.ToXmlString(false); // Obtiene la clave pública en formato XML
            byte[] publicKeyBytes = Encoding.UTF8.GetBytes(publicKey); // Codifica la clave pública en UTF-8



            //Dejo esto comentado.
            /*using (MemoryStream ms = new MemoryStream())
                {
                    DataContractSerializer serializer = new DataContractSerializer(typeof(RSAParameters));
                    serializer.WriteObject(ms, clavePublica);
                    rsabytes= ms.ToArray();
                    
                    // ahora puedes enviar los bytes a través del socket
                }*/



            // Data buffer for incoming data.  
            byte[] bytes = new byte[2048];
            
            // Connect to a remote device.  
            try {
                // Establish the remote endpoint for the socket.  
                // This example uses port 11000 on the local computer.  
                //IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
                //IPAddress ipAddress = ipHostInfo.AddressList[0];
                IPAddress ipAddress = getLocalIpAddress();//MAC OS
                IPEndPoint remoteEP = new IPEndPoint(ipAddress, 11000);

                // Create a TCP/IP  socket.
               
                Socket sender = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

                // Connect the socket to the remote endpoint. Catch any errors.  
                try {
                    sender.Connect(remoteEP);
                    //byte[] desencriptado = RSAalg.Decrypt(Encoding.UTF8.GetBytes(sender.RemoteEndPoint.ToString()), false);
                    //Console.WriteLine("Socket connected to {0}", Encoding.UTF8.GetString(desencriptado, 0, 2048));
                    

                    // Encode the data string into a byte array. 
                   

                    
                    // Send the data through the socket.  
                   // int bytesSent = sender.Send(publicKeyBytes); // Enviamos la clave publica al servidor
                    Console.WriteLine("Numero:");
                    byte[] msg = Encoding.ASCII.GetBytes(Console.ReadLine());
                    int bytesSent = sender.Send(msg);

                    int bytesRec = sender.Receive(bytes); 
                    Console.WriteLine("Echoed test = {0}", Encoding.ASCII.GetString(bytes, 0, bytesRec));

                   // byte[] desencriptado = RSAalg.Decrypt(Encoding.UTF8.GetBytes(sender.RemoteEndPoint.ToString()), false);
                   // Console.WriteLine("Socket connected to {0}", Encoding.UTF8.GetString(desencriptado, 0, 2048));

                    // Receive the response from the remote device.  
                    //int bytesRec = sender.Receive(bytes);
                    //Console.WriteLine("Echoed test = {0}", Encoding.ASCII.GetString(bytes, 0, bytesRec));

                    // Release the socket.  
                    sender.Shutdown(SocketShutdown.Both);
                    sender.Close();
                    
                } catch (ArgumentNullException ane) {
                    Console.WriteLine("ArgumentNullException : {0}", ane.ToString());
                } catch (SocketException se) {
                    Console.WriteLine("SocketException : {0}", se.ToString());
                } catch (Exception e) {
                    Console.WriteLine("Unexpected exception : {0}", e.ToString());
                }
               

            } catch (Exception e) {
                Console.WriteLine(e.ToString());
            }
            
        }

        static IPAddress getLocalIpAddress() {
            IPAddress ipAddress = null;
            try {
                foreach (var netInterface in NetworkInterface.GetAllNetworkInterfaces()) {
                    if (netInterface.NetworkInterfaceType == NetworkInterfaceType.Wireless80211 ||
                        netInterface.NetworkInterfaceType == NetworkInterfaceType.Ethernet) {
                        foreach (var addrInfo in netInterface.GetIPProperties().UnicastAddresses) {
                            if (addrInfo.Address.AddressFamily == AddressFamily.InterNetwork) {
                                ipAddress = addrInfo.Address;
                            }
                        }
                    }
                }
                if (ipAddress == null) {
                    IPHostEntry ipHostInfo = Dns.GetHostEntry("127.0.0.1");
                    ipAddress = ipHostInfo.AddressList[0];
                }
            } catch (Exception) { }
            return ipAddress;
        }

        public static int Main(String[] args) {
            //En MACOS porque no se puede ordenar el orden de arranque: servidor, cliente
            Thread.Sleep(4000);
            StartClient();
            return 0;
        }
    }
}
// > dotnet run
// Socket connected to 192.168.1.104:11000
// Echoed test = This is a test