using System.Net;
using System.Net.Sockets;
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
            Session session = new Session(clientSocket);
            session.Start();
            
            byte[] sendBuffer = "Welcome to MMORPG Server!"u8.ToArray();
            session.Send(sendBuffer);
            
            Thread.Sleep(1000);
            
            session.Disconnect();
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