using Framework.IO;

namespace HermesProxy.World.Server.Packets;

public struct MethodCall
{
	public ulong Type;

	public ulong ObjectId;

	public uint Token;

	public uint GetServiceHash()
	{
		return (uint)(Type >> 32);
	}

	public uint GetMethodId()
	{
		return (uint)(Type & 0xFFFFFFFFu);
	}

	public void SetServiceHash(uint serviceHash)
	{
		Type = (Type & 0xFFFFFFFFu) | ((ulong)serviceHash << 32);
	}

	public void SetMethodId(uint methodId)
	{
		Type = (Type & 0xFFFFFFFF00000000uL) | methodId;
	}

	public void Read(ByteBuffer data)
	{
		Type = data.ReadUInt64();
		ObjectId = data.ReadUInt64();
		Token = data.ReadUInt32();
	}

	public void Write(ByteBuffer data)
	{
		data.WriteUInt64(Type);
		data.WriteUInt64(ObjectId);
		data.WriteUInt32(Token);
	}
}
