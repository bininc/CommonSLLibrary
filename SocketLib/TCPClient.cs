using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using CommLiby;
using CommLiby.Cyhk;
using CommonLibSL.Model;
using Models;
using Newtonsoft.Json;
using CommLiby.SocketLib;

namespace CommonLibSL.SocketLib
{
    public class TCPClient : ITCPClient
    {
        public override ISocketAsyncEventArgs GetISocketAsyncEventArgs()
        {
            return new SLSocketAsyncEventArgs();
        }

        protected override ISocket GetISocket(CommLiby.SocketLib.AddressFamily addressFamily, CommLiby.SocketLib.SocketType socketType, CommLiby.SocketLib.ProtocolType protocolType)
        {
            return new SLSocket(addressFamily, socketType, protocolType);
        }
    }
}
