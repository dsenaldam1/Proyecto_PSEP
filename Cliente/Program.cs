using System;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Security.Cryptography;
using System.Collections;
using System.IO;

namespace clientesincrono {
    public class SynchronousSocketClient {

        public static void StartClient() {

            RSACryptoServiceProvider RSAalg = new RSACryptoServiceProvider(4096); //creamos el RSA en el cliente
            string publicKey = RSAalg.ToXmlString(false); // Obtiene la clave pública en formato XML
            byte[] publicKeyBytes = Encoding.UTF8.GetBytes(publicKey); // Codifica la clave pública en UTF-8
            string encriptado = ""; //Para guardar el archivo encriptado
            byte[] bytes = new byte[4096]; //Buffer
            
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
                    
                    // Send the data through the socket. 
                    //Se indica que seleccione un archivo y este se junta en una cadena con la clave y se envía al servidor 
                    Console.WriteLine("Selecciona un archivo entre el 1 y el 10:");
                    string num = (Console.ReadLine());
                    string cadena = (num + ":" + publicKey);
                    byte[] envio = Encoding.ASCII.GetBytes(cadena);

                    sender.SendAsync(new ArraySegment<byte>(envio), SocketFlags.None); 
                    
                    Thread.Sleep(4000); //Espera para dar tiempo a la llamada
                    //Se recoge y desencripta el mensaje enviado por el servidor
                    recibirDatos(sender, bytes);
                    Console.WriteLine(Encoding.UTF8.GetString(RSAalg.Decrypt(Encoding.UTF8.GetBytes(encriptado), false)));

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


            //Recive el archivo encriptado desde el servidor
            async void recibirDatos(Socket sender, byte[] buffer){
                var received = await sender.ReceiveAsync(buffer, SocketFlags.None);
                encriptado = Encoding.UTF8.GetString(buffer, 0, received);
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