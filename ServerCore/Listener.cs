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

        SocketAsyncEventArgs asyncEventArgs = new();
        asyncEventArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnAccept);
        RegisterAccept(asyncEventArgs);
    }

    private void RegisterAccept(SocketAsyncEventArgs asyncEventArgs)
    {
        asyncEventArgs.AcceptSocket = null;

        bool pending = _listenSocket.AcceptAsync(asyncEventArgs);
        if (pending == false)
            OnAccept(null, asyncEventArgs);
    }

    private void OnAccept(object? sender, SocketAsyncEventArgs asyncEventArgs)
    {
        if (asyncEventArgs.SocketError == SocketError.Success)
        {
            Socket? acceptSocket = asyncEventArgs.AcceptSocket;
            if (acceptSocket is null)
                throw new NullReferenceException($"{nameof(acceptSocket)} is null");

            _onAcceptHandler.Invoke(acceptSocket);
        }
        else
        {
            System.Console.WriteLine(asyncEventArgs.SocketError.ToString());
        }
    }
}
