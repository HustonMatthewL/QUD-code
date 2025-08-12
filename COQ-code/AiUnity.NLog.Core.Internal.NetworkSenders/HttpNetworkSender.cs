using System;
using System.IO;
using System.Net;
using AiUnity.NLog.Core.Common;

namespace AiUnity.NLog.Core.Internal.NetworkSenders
{
	internal class HttpNetworkSender : NetworkSender
	{
		public HttpNetworkSender(string url)
			: base(url)
		{
		}

		protected override void DoSend(byte[] bytes, int offset, int length, AsyncContinuation asyncContinuation)
		{
			WebRequest webRequest = WebRequest.Create(new Uri(base.Address));
			webRequest.Method = "POST";
			AsyncCallback onResponse = delegate(IAsyncResult r)
			{
				try
				{
					using (webRequest.EndGetResponse(r))
					{
					}
					asyncContinuation(null);
				}
				catch (Exception exception)
				{
					if (exception.MustBeRethrown())
					{
						throw;
					}
					asyncContinuation(exception);
				}
			};
			AsyncCallback callback = delegate(IAsyncResult r)
			{
				try
				{
					using (Stream stream = webRequest.EndGetRequestStream(r))
					{
						stream.Write(bytes, offset, length);
					}
					webRequest.BeginGetResponse(onResponse, null);
				}
				catch (Exception exception2)
				{
					if (exception2.MustBeRethrown())
					{
						throw;
					}
					asyncContinuation(exception2);
				}
			};
			webRequest.BeginGetRequestStream(callback, null);
		}
	}
}
