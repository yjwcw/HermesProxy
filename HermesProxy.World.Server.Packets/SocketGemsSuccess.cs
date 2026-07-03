using Framework.Constants;
using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

internal class SocketGemsSuccess : ServerPacket
{
	public WowGuid128 ItemGuid;

	public SocketGemsSuccess()
		: base(Opcode.SMSG_SOCKET_GEMS_SUCCESS, ConnectionType.Instance)
	{
	}

	public override void Write()
	{
		_worldPacket.WritePackedGuid128(ItemGuid);
	}
}
