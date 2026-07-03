using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

public class ClientCacheVersion : ServerPacket
{
	public uint CacheVersion;

	public ClientCacheVersion()
		: base(Opcode.SMSG_CACHE_VERSION)
	{
	}

	public override void Write()
	{
		_worldPacket.WriteUInt32(CacheVersion);
	}
}
