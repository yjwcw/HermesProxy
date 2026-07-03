using System;
using Framework.IO;

namespace HermesProxy.World;

public class PacketHeader
{
	public int Size;

	public byte[] Tag = new byte[12];

	public void Read(byte[] buffer)
	{
		Size = BitConverter.ToInt32(buffer, 0);
		Buffer.BlockCopy(buffer, 4, Tag, 0, 12);
	}

	public void Write(ByteBuffer byteBuffer)
	{
		byteBuffer.WriteInt32(Size);
		byteBuffer.WriteBytes(Tag, 12u);
	}

	public bool IsValidSize()
	{
		return Size < 262144;
	}
}
