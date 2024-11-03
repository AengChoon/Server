using System.Net.Sockets;
using System.Text;

namespace ServerCore;

public class Session(Socket socket)
{
    private int _disconnected;
    
    public void Start()
    {
        SocketAsyncEventArgs receiveEventArgs = new SocketAsyncEventArgs();
        receiveEventArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnReceive);
        receiveEventArgs.SetBuffer(new byte[1024], 0, 1024);
        
        RegisterReceive(receiveEventArgs);
    }

    public void Send(byte[] sendBuffer)
    {
        socket.Send(sendBuffer);
    }

    public void Disconnect()
    {
        if (Interlocked.Exchange(ref _disconnected, 1) == 1)
            return;
        
        socket.Shutdown(SocketShutdown.Both);
        socket.Close();
    }

    private void RegisterReceive(SocketAsyncEventArgs receiveEventArgs)
    {
        bool pending = socket.ReceiveAsync(receiveEventArgs);
        if (pending == false)
            OnReceive(null, receiveEventArgs);
    }

    private void OnReceive(object? sender, SocketAsyncEventArgs receiveEventArgs)
    {
        if (receiveEventArgs is { SocketError: SocketError.Success, BytesTransferred: > 0 })
        {
            if (receiveEventArgs.Buffer != null)
            {
                string receivedString = Encoding.UTF8.GetString(receiveEventArgs.Buffer, receiveEventArgs.Offset, receiveEventArgs.BytesTransferred);
                Console.WriteLine($"[From Client] {receivedString}");
            }
            
            RegisterReceive(receiveEventArgs);
        }
        else
        {
            
        }
    }
}