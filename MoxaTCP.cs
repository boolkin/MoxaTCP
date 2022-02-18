using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Linq;

namespace MoxaTCP {
    class MoxaTCP {

		private static byte[] commonBytes = new byte[27];
		private static int deviceNumb = 1;
		private static byte[] device1 = {0x01,0x03,0x00,0x14,0x00,0x02,0x84,0x0F}; 
		private static byte[] device2 = {0x02,0x03,0x00,0x14,0x00,0x02,0x84,0x3C};
		private static byte[] device3 = {0x03,0x03,0x00,0x14,0x00,0x02,0x85,0xED};
		
        [STAThread]
        static void Main(string[] args) {
            try
			  {

				// вводится адрес и порт устройства МОХА
				TcpClient client = new TcpClient("192.168.0.1", 4002);
				NetworkStream stream = client.GetStream();

				// сразу же делаем первый запрос
				if (deviceNumb==1) {
					stream.Write(device1, 0, device1.Length);
				}
				while (true) {
					// Buffer to store the response bytes.
					Byte[] data = new Byte[9];
					stream.Read(data, 0, data.Length);
					
					if (data.Length > 0 && deviceNumb==1){
						//копируем в общий массив байт
						for (int i = 0; i<data.Length;i++) {
							commonBytes[i] = data[i];
						}
						deviceNumb = 2;
						stream.Write(device2, 0, device2.Length);
						continue;
					}
					
					if (data.Length > 0 && deviceNumb==2){
						//копируем в общий массив байт
						for (int i = 0; i<data.Length;i++) {
							commonBytes[i+9] = data[i];
						}
						stream.Write(device3, 0, device3.Length);
						deviceNumb = 3;					
						continue;
					}
					
					if (data.Length > 0 && deviceNumb==3){
						//копируем в общий массив байт
						for (int i = 0; i<data.Length;i++) {
							commonBytes[i+18] = data[i];
						}
						// посылаем общую строку на удаленный сервер по UDP
						UDPSend(commonBytes, "127.0.0.1", 5011);
						//Thread.Sleep(1000);
						deviceNumb = 1;
						/*	StringBuilder hex = new StringBuilder(commonBytes.Length * 2);
							foreach(byte b in commonBytes)
									hex.AppendFormat("{0:x2}", b);		
							Console.WriteLine("commonBytes 3: {0}", hex.ToString());*/
						stream.Write(device1, 0, device1.Length);
						continue;
					}
					// Close everything.
					stream.Close();
					client.Close();
				}
			  }

			  catch (SocketException e)
			  {
				Console.WriteLine("SocketException: {0}", e);
			  }
				catch (IOException e)
			  {
				Console.WriteLine("Разрыв соединения: {0}", e);
			  }

			  Console.WriteLine("\n Press Enter to continue...");
			  Console.Read();
        }
		
		private static void UDPSend(byte[] datagram, string IPaddr, int remPort) {
            // Создаем UdpClient
            UdpClient sender = new UdpClient();
			IPAddress remoteIPAddress = IPAddress.Parse(IPaddr);
			int remotePort = Convert.ToInt16(remPort);
            // Создаем endPoint по информации об удаленном хосте
            IPEndPoint endPoint = new IPEndPoint(remoteIPAddress, remotePort);

            try
            {
                sender.Send(datagram, datagram.Length, endPoint);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Возникло исключение: " + ex.ToString() + "\n  " + ex.Message);
            }
            finally
            {
                // Закрыть соединение
                sender.Close();
            }
        }
    }
}