using System;
using Framework.IO;
using Framework.Util;

namespace HermesProxy.World;

public class LegacyClientPacketHeader
{
	public const int StructSize = 6;

	public ushort Size;

	public uint Opcode;

	public void Read(byte[] buffer)
	{
		Size = BitConverter.ToUInt16(buffer, 0);
		Opcode = BitConverter.ToUInt32(buffer, 2);
	}

	public void Write(ByteBuffer byteBuffer)
	{
		byteBuffer.WriteUInt16(NetworkUtility.EndianConvert(Size));
		byteBuffer.WriteUInt32(Opcode);
	}
}
