using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using AiUnity.NLog.Core.Common;

namespace AiUnity.NLog.Core.Internal.NetworkSenders
{
	internal class UdpNetworkSender : NetworkSender
	{
		private ISocket socket;

		private EndPoint endpoint;

		internal AddressFamily AddressFamily { get; set; }

		public UdpNetworkSender(string url, AddressFamily addressFamily)
			: base(url)
		{
			AddressFamily = addressFamily;
		}

		protected internal virtual ISocket CreateSocket(AddressFamily addressFamily, SocketType socketType, ProtocolType protocolType)
		{
			SocketProxy socketProxy = new SocketProxy(addressFamily, socketType, protocolType);
			if (Uri.TryCreate(base.Address, UriKind.Absolute, out var result) && result.Host.Equals(IPAddress.Broadcast.ToString(), StringComparison.InvariantCultureIgnoreCase))
			{
				socketProxy.UnderlyingSocket.EnableBroadcast = true;
			}
			return socketProxy;
		}

		protected override void DoInitialize()
		{
			endpoint = ParseEndpointAddress(new Uri(base.Address), AddressFamily);
			socket = CreateSocket(endpoint.AddressFamily, SocketType.Dgram, ProtocolType.Udp);
		}

		protected override void DoClose(AsyncContinuation continuation)
		{
			lock (this)
			{
				try
				{
					if (socket != null)
					{
						socket.Close();
					}
				}
				catch (Exception exception)
				{
					if (exception.MustBeRethrown())
					{
						throw;
					}
				}
				socket = null;
			}
		}

		protected override void DoSend(byte[] bytes, int offset, int length, AsyncContinuation asyncContinuation)
		{
			lock (this)
			{
				SocketAsyncEventArgs socketAsyncEventArgs = new SocketAsyncEventArgs();
				socketAsyncEventArgs.SetBuffer(bytes, offset, length);
				socketAsyncEventArgs.UserToken = asyncContinuation;
				socketAsyncEventArgs.Completed += SocketOperationCompleted;
				socketAsyncEventArgs.RemoteEndPoint = endpoint;
				if (!socket.SendToAsync(socketAsyncEventArgs))
				{
					SocketOperationCompleted(socket, socketAsyncEventArgs);
				}
			}
		}

		private void SocketOperationCompleted(object sender, SocketAsyncEventArgs e)
		{
			AsyncContinuation asyncContinuation = e.UserToken as AsyncContinuation;
			Exception exception = null;
			if (e.SocketError != 0)
			{
				exception = new IOException("Error: " + e.SocketError);
			}
			e.Dispose();
			asyncContinuation?.Invoke(exception);
		}

		public override void CheckSocket()
		{
			if (socket == null)
			{
				DoInitialize();
			}
		}
	}
}
