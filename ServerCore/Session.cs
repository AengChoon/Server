using System.Net.Sockets;
using System.Text;

namespace ServerCore;

public class Session(Socket socket)
{
    private int _disconnected;
    private readonly object _sendLock = new object();
    private readonly Queue<byte[]> _sendQueue = new Queue<byte[]>();
    private readonly List<ArraySegment<byte>> _sendBufferList = [];
    private readonly SocketAsyncEventArgs _sendEventArgs = new SocketAsyncEventArgs();
    private readonly SocketAsyncEventArgs _receiveEventArgs = new SocketAsyncEventArgs();
    
    public void Start()
    {
        _sendEventArgs.Completed += OnSend;
        
        _receiveEventArgs.Completed += OnReceive;
        _receiveEventArgs.SetBuffer(new byte[1024], 0, 1024);
        
        RegisterReceive();
    }

    public void Send(byte[] sendBuffer)
    {
        lock (_sendLock)
        {
            _sendQueue.Enqueue(sendBuffer);
            if (_sendBufferList.Count == 0)
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
        while (_sendQueue.Count > 0)
        {
            byte[] sendBuffer = _sendQueue.Dequeue();
            _sendBufferList.Add(new ArraySegment<byte>(sendBuffer, 0, sendBuffer.Length));
        }
        _sendEventArgs.BufferList = _sendBufferList;
        
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
                _sendEventArgs.BufferList = null;
                _sendBufferList.Clear();
                
                if (_sendQueue.Count > 0)
                    RegisterSend();
            }
            else
            {
                Disconnect();
            }
        }
    }

    private void RegisterReceive()
    {
        bool pending = socket.ReceiveAsync(_receiveEventArgs);
        if (pending == false)
            OnReceive(null, _receiveEventArgs);
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
            
            RegisterReceive();
        }
        else
        {
            Disconnect();
        }
    }
}