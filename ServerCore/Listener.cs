using System.Net;
using System.Net.Sockets;

namespace ServerCore;

public class Listener
{
    private readonly Socket _listenSocket;
    private readonly Action<Socket> _onAcceptHandler;

    public Listener(IPEndPoint endPoint, Action<Socket> onAcceptHandler)
    {
        _listenSocket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
        _onAcceptHandler += onAcceptHandler;

        _listenSocket.Bind(endPoint);
    }

    public void Listen()
    {
        _listenSocket.Listen(10);

        SocketAsyncEventArgs acceptEventArgs = new SocketAsyncEventArgs();
        acceptEventArgs.Completed += OnAccept;
        RegisterAccept(acceptEventArgs);
    }

    private void RegisterAccept(SocketAsyncEventArgs acceptEventArgs)
    {
        acceptEventArgs.AcceptSocket = null;

        bool pending = _listenSocket.AcceptAsync(acceptEventArgs);
        if (pending == false)
            OnAccept(null, acceptEventArgs);
    }

    private void OnAccept(object? sender, SocketAsyncEventArgs acceptEventArgs)
    {
        if (acceptEventArgs.SocketError == SocketError.Success)
        {
            Socket? acceptSocket = acceptEventArgs.AcceptSocket;
            if (acceptSocket is null)
                throw new NullReferenceException($"{nameof(acceptSocket)} is null");

            _onAcceptHandler.Invoke(acceptSocket);
        }
        else
        {
            Console.WriteLine(acceptEventArgs.SocketError.ToString());
        }
        
        RegisterAccept(acceptEventArgs);
    }
}
