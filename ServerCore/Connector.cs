using System.Net;
using System.Net.Sockets;

namespace ServerCore;

public class Connector
{
    private Func<Socket, Session>? _sessionFactory;
    
    public void Connect(IPEndPoint endPoint, Func<Socket, Session>? sessionFactory)
    {
        Socket socket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
        _sessionFactory = sessionFactory;
        
        SocketAsyncEventArgs connectEventArgs = new SocketAsyncEventArgs();
        connectEventArgs.Completed += OnConnect;
        connectEventArgs.RemoteEndPoint = endPoint;
        connectEventArgs.UserToken = socket;
        
        RegisterConnect(connectEventArgs);
    }

    private void RegisterConnect(SocketAsyncEventArgs connectEventArgs)
    {
        if (connectEventArgs.UserToken is not Socket socket)
            return;
        
        bool pending = socket.ConnectAsync(connectEventArgs);
        if (pending == false)
            OnConnect(null, connectEventArgs);
    }

    private void OnConnect(object? sender, SocketAsyncEventArgs connectEventArgs)
    {
        if (connectEventArgs.SocketError == SocketError.Success && _sessionFactory != null)
        {
            Session session = _sessionFactory.Invoke(connectEventArgs.ConnectSocket!);
            session.Start();
            session.OnConnected(connectEventArgs.RemoteEndPoint!);
        }
        else
        {
            Console.WriteLine("OnConnect failed: " + connectEventArgs.SocketError);
        }
    }
}