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
using AddressFamily = System.Net.Sockets.AddressFamily;
using SocketAsyncOperation = CommLiby.SocketLib.SocketAsyncOperation;
using SocketError = CommLiby.SocketLib.SocketError;


namespace CommonLibSL.SocketLib
{
    public class SLSocketAsyncEventArgs : ISocketAsyncEventArgs
    {
        private readonly SocketAsyncEventArgs _socketAsyncEventArgs;
        private readonly IPEndPoint _remoteEntPonit;
        public SLSocketAsyncEventArgs()
        {
            _socketAsyncEventArgs = new SocketAsyncEventArgs();
            _socketAsyncEventArgs.Completed += _socketAsyncEventArgs_Completed;
            _remoteEntPonit = new IPEndPoint(IPAddress.None, 0);
        }

        private void _socketAsyncEventArgs_Completed(object sender, SocketAsyncEventArgs e)
        {
            OnCompleted(this);
        }

        public override SocketProtocol SocketProtocol
        {
            get
            {
                return (SocketProtocol)(int)_socketAsyncEventArgs.SocketClientAccessPolicyProtocol;
            }
            set
            {
                _socketAsyncEventArgs.SocketClientAccessPolicyProtocol =
                    (SocketClientAccessPolicyProtocol)(int)value;
            }
        }

        public override object SocketAsyncEventArgs => _socketAsyncEventArgs;

        public override SocketError SocketError
        {
            get { return (SocketError)(int)_socketAsyncEventArgs.SocketError; }
            set { _socketAsyncEventArgs.SocketError = (System.Net.Sockets.SocketError)(int)value; }
        }

        public override SocketAsyncOperation LastOperation => (SocketAsyncOperation)(int)_socketAsyncEventArgs.LastOperation;
        public override string RemoteIP
        {
            get
            {
                IPEndPoint ipEndPoint = _socketAsyncEventArgs.RemoteEndPoint as IPEndPoint;
                return ipEndPoint?.Address.ToString();
            }
            set
            {
                if (string.IsNullOrWhiteSpace(value)) return;
                IPAddress ipAddress = IPAddress.Parse(value);
                _remoteEntPonit.Address = ipAddress;

                if (!_remoteEntPonit.Address.Equals(IPAddress.None) && _remoteEntPonit.Port != 0)
                    _socketAsyncEventArgs.RemoteEndPoint = _remoteEntPonit;
            }
        }

        public override int RemotePort
        {
            get
            {
                IPEndPoint ipEndPoint = _socketAsyncEventArgs.RemoteEndPoint as IPEndPoint;
                if (ipEndPoint == null)
                {
                    return 0;
                }
                return ipEndPoint.Port;
            }
            set
            {
                if (value < 0) value = 0;

                _remoteEntPonit.Port = value;

                if (!_remoteEntPonit.Address.Equals(IPAddress.None) && _remoteEntPonit.Port != 0)
                    _socketAsyncEventArgs.RemoteEndPoint = _remoteEntPonit;
            }
        }

        public override object UserToken
        {
            get { return _socketAsyncEventArgs.UserToken; }
            set { _socketAsyncEventArgs.UserToken = value; }
        }

        public override void SetBuffer(byte[] buffer, int offset, int count)
        {
            _socketAsyncEventArgs.SetBuffer(buffer, offset, count);
        }

        public override int BytesTransferred
        {
            get
            {
                return _socketAsyncEventArgs.BytesTransferred;
            }
        }

        public override void Dispose()
        {
            _socketAsyncEventArgs.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
