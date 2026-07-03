using System.Collections.Generic;
using Framework.IO;
using HermesProxy.World.Client;
using HermesProxy.World.Enums;
using Ionic.Zlib;

namespace HermesProxy.World;

public class WorldPacket : ByteBuffer
{
	private uint opcode;

	private long m_receivedTime;

	public WorldPacket(uint opcode = 0u)
	{
		this.opcode = opcode;
	}

	public WorldPacket(Opcode opcode)
	{
		this.opcode = LegacyVersion.GetCurrentOpcode(opcode);
	}

	public WorldPacket(uint opcode, byte[] data)
		: base(data)
	{
		this.opcode = opcode;
	}

	public WorldPacket(byte[] data)
		: base(data)
	{
		opcode = ReadUInt16();
	}

	public KeyValuePair<int, bool> ReadEntry()
	{
		uint num = ReadUInt32();
		uint num2 = num & 0x7FFFFFFFu;
		return new KeyValuePair<int, bool>((int)num2, num2 != num);
	}

	public WowGuid64 ReadGuid()
	{
		return new WowGuid64(ReadUInt64());
	}

	public WowGuid64 ReadPackedGuid()
	{
		return new WowGuid64(ReadPackedUInt64(ReadUInt8()));
	}

	public WowGuid128 ReadPackedGuid128()
	{
		byte length = ReadUInt8();
		byte length2 = ReadUInt8();
		ulong low = ReadPackedUInt64(length);
		return new WowGuid128(ReadPackedUInt64(length2), low);
	}

	private ulong ReadPackedUInt64(byte length)
	{
		if (length == 0)
		{
			return 0uL;
		}
		ulong num = 0uL;
		for (int i = 0; i < 8; i++)
		{
			if (((1 << i) & length) != 0)
			{
				num |= (ulong)ReadUInt8() << i * 8;
			}
		}
		return num;
	}

	public UpdateField ReadUpdateField()
	{
		uint val = ReadUInt32();
		return new UpdateField(val);
	}

	public WorldPacket Inflate(int inflatedSize)
	{
		byte[] array = ReadToEnd();
		byte[] array2 = new byte[inflatedSize];
		ZlibCodec zlibCodec = new ZlibCodec(CompressionMode.Decompress);
		zlibCodec.InputBuffer = array;
		zlibCodec.NextIn = 0;
		zlibCodec.AvailableBytesIn = array.Length;
		zlibCodec.OutputBuffer = array2;
		zlibCodec.NextOut = 0;
		zlibCodec.AvailableBytesOut = inflatedSize;
		zlibCodec.Inflate(FlushType.None);
		zlibCodec.Inflate(FlushType.Finish);
		zlibCodec.EndInflate();
		WorldPacket worldPacket = new WorldPacket(GetOpcode(), array2);
		worldPacket.SetReceiveTime(GetReceivedTime());
		return worldPacket;
	}

	public void WriteGuid(WowGuid64 guid)
	{
		WriteUInt64(guid.GetLowValue());
	}

	public void WritePackedGuid(WowGuid64 guid)
	{
		WritePackedUInt64(guid.Low);
	}

	public void WritePackedGuid128(WowGuid128 guid)
	{
		if (guid.IsEmpty())
		{
			WriteUInt8(0);
			WriteUInt8(0);
			return;
		}
		byte mask;
		byte[] result;
		uint count = PackUInt64(guid.GetLowValue(), out mask, out result);
		byte mask2;
		byte[] result2;
		uint count2 = PackUInt64(guid.GetHighValue(), out mask2, out result2);
		WriteUInt8(mask);
		WriteUInt8(mask2);
		WriteBytes(result, count);
		WriteBytes(result2, count2);
	}

	public void WritePackedUInt64(ulong guid)
	{
		byte mask;
		byte[] result;
		uint count = PackUInt64(guid, out mask, out result);
		WriteUInt8(mask);
		WriteBytes(result, count);
	}

	private uint PackUInt64(ulong value, out byte mask, out byte[] result)
	{
		uint result2 = 0u;
		mask = 0;
		result = new byte[8];
		byte b = 0;
		while (value != 0L)
		{
			if ((value & 0xFF) != 0L)
			{
				mask |= (byte)(1 << (int)b);
				result[result2++] = (byte)(value & 0xFF);
			}
			value >>= 8;
			b++;
		}
		return result2;
	}

	public void WriteBytes(WorldPacket data)
	{
		FlushBits();
		WriteBytes(data.GetData());
	}

	public uint GetOpcode()
	{
		return opcode;
	}

	public Opcode GetUniversalOpcode(bool isModern)
	{
		if (isModern)
		{
			return ModernVersion.GetUniversalOpcode(GetOpcode());
		}
		return LegacyVersion.GetUniversalOpcode(GetOpcode());
	}

	public long GetReceivedTime()
	{
		return m_receivedTime;
	}

	public void SetReceiveTime(long receivedTime)
	{
		m_receivedTime = receivedTime;
	}
}
