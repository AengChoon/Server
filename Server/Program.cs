using System.Net;
using System.Net.Sockets;
using System.Text;
using ServerCore;

namespace Server;

class GameSession(Socket socket) : Session(socket)
{
    public override void OnConnected(EndPoint endPoint)
    {
        Console.WriteLine($"Connected: {endPoint}");
        
        byte[] sendBuffer = "Welcome to MMORPG Server!"u8.ToArray();
        Send(sendBuffer);
            
        Thread.Sleep(1000);
            
        Disconnect();
    }

    protected override void OnDisconnected(EndPoint endPoint)
    {
        Console.WriteLine($"Disconnected: {endPoint}");
    }

    protected override void OnSend(int bytesSent)
    {
        Console.WriteLine($"Bytes sent: {bytesSent}");
    }

    protected override void OnReceive(ArraySegment<byte> buffer)
    {
        if (buffer.Array == null)
            return;
        
        string receivedString = Encoding.UTF8.GetString(buffer.Array, buffer.Offset, buffer.Count);
        Console.WriteLine($"[From Client] {receivedString}");
    }
}

internal static class Program
{
    private static readonly Listener Listener;

    static Program()
    {
        string host = Dns.GetHostName();
        IPHostEntry ipEntry = Dns.GetHostEntry(host);
        IPAddress ipAddress = ipEntry.AddressList[0];
        IPEndPoint endPoint = new IPEndPoint(ipAddress, 7777);
        
        Listener = new Listener(endPoint, (Socket clientSocket) => new GameSession(clientSocket));
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