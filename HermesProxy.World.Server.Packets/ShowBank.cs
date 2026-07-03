using Framework.Constants;
using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

public class ShowBank : ServerPacket
{
	public WowGuid128 Guid;

	public ShowBank()
		: base(Opcode.SMSG_SHOW_BANK, ConnectionType.Instance)
	{
	}

	public override void Write()
	{
		_worldPacket.WritePackedGuid128(Guid);
	}
}
