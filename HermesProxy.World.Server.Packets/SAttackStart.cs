using Framework.Constants;
using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

public class SAttackStart : ServerPacket
{
	public WowGuid128 Attacker;

	public WowGuid128 Victim;

	public SAttackStart()
		: base(Opcode.SMSG_ATTACK_START, ConnectionType.Instance)
	{
	}

	public override void Write()
	{
		_worldPacket.WritePackedGuid128(Attacker);
		_worldPacket.WritePackedGuid128(Victim);
	}
}
