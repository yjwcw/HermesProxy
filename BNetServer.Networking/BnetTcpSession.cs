using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Bgs.Protocol;
using BNetServer.Services;
using Framework.Constants;
using Framework.IO;
using Framework.Logging;
using Framework.Networking;
using Google.Protobuf;

namespace BNetServer.Networking;

public class BnetTcpSession : SSLSocket, BnetServices.INetwork
{
	private readonly BnetServices.ServiceManager _handlerManager;

	private List<byte> _currentBuffer = new List<byte>();

	public BnetTcpSession(Socket socket)
		: base(socket)
	{
		_handlerManager = new BnetServices.ServiceManager("BnetTcp", this, null);
	}

	public override void Accept()
	{
		string text = GetRemoteIpEndPoint().ToString();
		Log.Print(LogType.Server, "接受来自的连接 " + text + ".", "Accept", "D:\\a\\HermesProxy\\HermesProxy\\BnetServer\\Networking\\BnetTcpSession.cs");
		AsyncHandshake(BnetServerCertificate.Certificate);
	}

	public override bool Update()
	{
		if (!base.Update())
		{
			return false;
		}
		return true;
	}

	public override async Task ReadHandler(byte[] data, int receivedLength)
	{
		if (IsOpen())
		{
			_currentBuffer.AddRange(data.Take(receivedLength));
			await ProcessCurrentBuffer();
			await AsyncRead();
		}
	}

	private Task ProcessCurrentBuffer()
	{
		while (_currentBuffer.Count > 2)
		{
			ushort num = (ushort)IPAddress.HostToNetworkOrder(BitConverter.ToInt16(_currentBuffer.Take(2).ToArray()));
			if (_currentBuffer.Count < 2 + num)
			{
				return Task.CompletedTask;
			}
			byte[] data = _currentBuffer.Skip(2).Take(num).ToArray();
			Header header = new Header();
			header.MergeFrom(data);
			int size = (int)header.Size;
			if (_currentBuffer.Count < 2 + num + size)
			{
				return Task.CompletedTask;
			}
			byte[] buffer = _currentBuffer.Skip(2).Skip(num).Take(size)
				.ToArray();
			_currentBuffer.RemoveRange(0, 2 + num + (int)header.Size);
			CodedInputStream stream = new CodedInputStream(buffer);
			if (header.ServiceId != 254 && header.ServiceHash != 0)
			{
				_handlerManager.Invoke(header.ServiceId, (OriginalHash)header.ServiceHash, header.MethodId, header.Token, stream);
			}
		}
		return Task.CompletedTask;
	}

	public void SendRpcMessage(uint serviceId, OriginalHash service, uint methodId, uint token, BattlenetRpcErrorCode status, IMessage? message)
	{
		Header header = new Header();
		header.Token = token;
		header.Status = (uint)status;
		header.ServiceId = serviceId;
		header.ServiceHash = (uint)service;
		header.MethodId = methodId;
		if (message != null)
		{
			header.Size = (uint)message.CalculateSize();
		}
		ByteBuffer byteBuffer = new ByteBuffer();
		byteBuffer.WriteBytes(GetHeaderSize(header), 2u);
		byteBuffer.WriteBytes(header.ToByteArray());
		if (message != null)
		{
			byteBuffer.WriteBytes(message.ToByteArray());
		}
		AsyncWrite(byteBuffer.GetData());
	}

	public byte[] GetHeaderSize(Header header)
	{
		ushort num = (ushort)header.CalculateSize();
		byte[] result = new byte[2]
		{
			(byte)((uint)(num >> 8) & 0xFFu),
			(byte)(num & 0xFFu)
		};
		Array.Reverse(BitConverter.GetBytes((ushort)header.CalculateSize()));
		return result;
	}

	IPEndPoint BnetServices.INetwork.GetRemoteIpEndPoint()
	{
		return GetRemoteIpEndPoint();
	}
}
