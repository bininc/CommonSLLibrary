using System;
using System.Net;
using System.Net.Sockets;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using CommLiby.SocketLib;
using AddressFamily = CommLiby.SocketLib.AddressFamily;
using ProtocolType = CommLiby.SocketLib.ProtocolType;
using SocketShutdown = CommLiby.SocketLib.SocketShutdown;
using SocketType = CommLiby.SocketLib.SocketType;

namespace CommonLibSL.SocketLib
{
    public class SLSocket : ISocket
    {
        private readonly Socket _socket;
        public SLSocket(AddressFamily addressFamily, SocketType socketType, ProtocolType protocolType) : base(addressFamily, socketType, protocolType)
        {
            _socket = new Socket((System.Net.Sockets.AddressFamily)(int)addressFamily,
                (System.Net.Sockets.SocketType)(int)socketType,
                (System.Net.Sockets.ProtocolType)(int)protocolType);
        }

        public override int SendBufferSize
        {
            get { return _socket.SendBufferSize; }
            set { _socket.SendBufferSize = value; }
        }

        public override int ReceiveBufferSize
        {
            get { return _socket.ReceiveBufferSize; }
            set { _socket.ReceiveBufferSize = value; }
        }
        public override short Ttl
        {
            get { return _socket.Ttl; }
            set { _socket.Ttl = value; }
        }
        public override bool ConnectAsync(ISocketAsyncEventArgs e)
        {
            SocketAsyncEventArgs ee = (SocketAsyncEventArgs)e.SocketAsyncEventArgs;
            return _socket.ConnectAsync(ee);
        }

        public override bool ReceiveAsync(ISocketAsyncEventArgs e)
        {
            SocketAsyncEventArgs ee = (SocketAsyncEventArgs)e.SocketAsyncEventArgs;
            return _socket.ReceiveAsync(ee);
        }

        public override void Shutdown(SocketShutdown how)
        {
            _socket.Shutdown((System.Net.Sockets.SocketShutdown)(int)how);
        }

        public override void Dispose()
        {
            _socket.Dispose();
            GC.SuppressFinalize(this);
        }

        public override void Close()
        {
            _socket.Close();
        }

        public override bool SendAsync(ISocketAsyncEventArgs e)
        {
            SocketAsyncEventArgs ee = (SocketAsyncEventArgs)e.SocketAsyncEventArgs;
            return _socket.SendAsync(ee);
        }
    }
}
