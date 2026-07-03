using Framework.Constants;
using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

public class CooldownCheat : ServerPacket
{
	public WowGuid128 Guid;

	public CooldownCheat()
		: base(Opcode.SMSG_COOLDOWN_CHEAT, ConnectionType.Instance)
	{
	}

	public override void Write()
	{
		_worldPacket.WritePackedGuid128(Guid);
	}
}
