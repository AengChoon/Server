using System.Net;
using System.Net.Sockets;
using System.Text;

namespace DummyClient;

internal static class Program
{
    public static void Main(string[] args)
    {
        string host = Dns.GetHostName();
        IPHostEntry ipEntry = Dns.GetHostEntry(host);
        IPAddress ipAddress = ipEntry.AddressList[0];
        IPEndPoint endPoint = new IPEndPoint(ipAddress, 7777);

        while (true)
        {
            Socket socket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            try
            {
                socket.Connect(endPoint);
                Console.WriteLine("Socket connected to {0}:{1}", endPoint.Address, endPoint.Port);
            
                byte[] sendBuffer = "Hello World!"u8.ToArray();
                int bytesSent = socket.Send(sendBuffer);
            
                byte[] receiveBuffer = new byte[1024];
                int bytesReceived = socket.Receive(receiveBuffer);
                string receivedString = Encoding.UTF8.GetString(receiveBuffer, 0, bytesReceived);
                Console.WriteLine($"[From Server] {receivedString}");
            
                socket.Shutdown(SocketShutdown.Both);
                socket.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            
            Thread.Sleep(1000);
        }
    }
}