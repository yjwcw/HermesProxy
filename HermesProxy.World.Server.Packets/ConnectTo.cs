using System;
using System.Linq;
using System.Security.Cryptography;
using Framework.Constants;
using Framework.Cryptography;
using Framework.IO;
using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

internal class ConnectTo : ServerPacket
{
	public class ConnectPayload
	{
		public SocketAddress Where;

		public ushort Port;

		public byte[] Signature = new byte[256];
	}

	public struct SocketAddress
	{
		public AddressType Type;

		public byte[] IPv4;

		public byte[] IPv6;

		public string NameSocket;
	}

	public enum AddressType
	{
		IPv4 = 1,
		IPv6,
		NamedSocket
	}

	public ulong Key;

	public ConnectToSerial Serial;

	public ConnectPayload Payload;

	public byte Con;

	public ConnectTo()
		: base(Opcode.SMSG_CONNECT_TO)
	{
		Payload = new ConnectPayload();
	}

	public override void Write()
	{
		ByteBuffer byteBuffer = new ByteBuffer();
		byteBuffer.WriteUInt8((byte)Payload.Where.Type);
		switch (Payload.Where.Type)
		{
		case AddressType.IPv4:
			byteBuffer.WriteBytes(Payload.Where.IPv4);
			break;
		case AddressType.IPv6:
			byteBuffer.WriteBytes(Payload.Where.IPv6);
			break;
		case AddressType.NamedSocket:
			byteBuffer.WriteString(Payload.Where.NameSocket);
			break;
		}
		Sha256 sha = new Sha256();
		sha.Process(byteBuffer.GetData(), (int)byteBuffer.GetSize());
		sha.Process((uint)Payload.Where.Type);
		sha.Finish(BitConverter.GetBytes(Payload.Port));
		Payload.Signature = RsaCrypt.RSA.SignHash(sha.Digest, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1).Reverse().ToArray();
		_worldPacket.WriteBytes(Payload.Signature, (uint)Payload.Signature.Length);
		_worldPacket.WriteBytes(byteBuffer);
		_worldPacket.WriteUInt16(Payload.Port);
		_worldPacket.WriteUInt32((uint)Serial);
		_worldPacket.WriteUInt8(Con);
		_worldPacket.WriteUInt64(Key);
	}
}
