using System.Net;
using System.Net.Sockets;
using System.Text;
using ServerCore;

namespace Server;

internal static class Program
{
    private static readonly Listener Listener;

    static Program()
    {
        string host = Dns.GetHostName();
        IPHostEntry ipEntry = Dns.GetHostEntry(host);
        IPAddress ipAddress = ipEntry.AddressList[0];
        IPEndPoint endPoint = new IPEndPoint(ipAddress, 7777);
        
        Listener = new Listener(endPoint, OnAcceptHandler);
    }

    private static void OnAcceptHandler(Socket clientSocket)
    {
        try
        {
            byte[] receiveBuffer = new byte[1024];
            int receivedBytes = clientSocket.Receive(receiveBuffer);
            string receivedString = Encoding.UTF8.GetString(receiveBuffer, 0, receivedBytes);
            Console.WriteLine($"[From Client] {receivedString}");

            byte[] sendBuffer = "Welcome to MMORPG Server!"u8.ToArray();
            clientSocket.Send(sendBuffer);

            clientSocket.Shutdown(SocketShutdown.Both);
            clientSocket.Close();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }
    
    public static void Main(string[] args)
    {
        Listener.Listen();
        Console.WriteLine("Listening...");

        while (true)
        {
            
        }
    }
}