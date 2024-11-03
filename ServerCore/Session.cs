using System.Net.Sockets;
using System.Text;

namespace ServerCore;

public class Session(Socket socket)
{
    private int _disconnected;
    private bool _isSending;
    private readonly Queue<byte[]> _sendQueue = new Queue<byte[]>();
    private readonly object _sendLock = new object();
    private readonly SocketAsyncEventArgs _sendEventArgs = new SocketAsyncEventArgs();
    
    public void Start()
    {
        _sendEventArgs.Completed += OnSend;
        
        SocketAsyncEventArgs receiveEventArgs = new SocketAsyncEventArgs();
        receiveEventArgs.Completed += OnReceive;
        receiveEventArgs.SetBuffer(new byte[1024], 0, 1024);
        
        RegisterReceive(receiveEventArgs);
    }

    public void Send(byte[] sendBuffer)
    {
        lock (_sendLock)
        {
            _sendQueue.Enqueue(sendBuffer);
            if (_isSending == false)
                RegisterSend();
        }
    }

    public void Disconnect()
    {
        if (Interlocked.Exchange(ref _disconnected, 1) == 1)
            return;
        
        socket.Shutdown(SocketShutdown.Both);
        socket.Close();
    }

    private void RegisterSend()
    {
        _isSending = true;
        byte[] sendBuffer = _sendQueue.Dequeue();
        _sendEventArgs.SetBuffer(sendBuffer, 0, sendBuffer.Length);
        
        bool pending = socket.SendAsync(_sendEventArgs);
        if (pending == false)
            OnSend(null, _sendEventArgs);
    }

    private void OnSend(object? sender, SocketAsyncEventArgs sendEventArgs)
    {
        lock (_sendLock)
        {
            if (sendEventArgs is { SocketError: SocketError.Success, BytesTransferred: > 0 })
            {
                if (_sendQueue.Count > 0)
                    RegisterSend();
                else 
                    _isSending = false;
            }
            else
            {
                Disconnect();
            }
        }
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
            Disconnect();
        }
    }
}