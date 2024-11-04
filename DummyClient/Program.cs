using System.Net;
using System.Net.Sockets;
using System.Text;
using ServerCore;

namespace DummyClient;

internal class GameSession(Socket socket) : Session(socket)
{
    public override void OnConnected(EndPoint endPoint)
    {
        Console.WriteLine($"Connected: {endPoint}");
        
        for (int i = 0; i < 5; i++)
        {
            byte[] sendBuffer = Encoding.UTF8.GetBytes($"Hello World! {i}");
            Send(sendBuffer);
        }
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
        Console.WriteLine($"[From Server] {receivedString}");
    }
}

internal static class Program
{
    public static void Main(string[] args)
    {
        string host = Dns.GetHostName();
        IPHostEntry ipEntry = Dns.GetHostEntry(host);
        IPAddress ipAddress = ipEntry.AddressList[0];
        IPEndPoint endPoint = new IPEndPoint(ipAddress, 7777);
        
        Connector connector = new Connector();
        connector.Connect(endPoint, (Socket clientSocket) => new GameSession(clientSocket));

        while (true)
        {
            try
            {
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            
            Thread.Sleep(1000);
        }
    }
}