using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

public class AttackSwingError : ServerPacket
{
	public AttackSwingErr Reason;

	public AttackSwingError()
		: base(Opcode.SMSG_ATTACK_SWING_ERROR)
	{
	}

	public override void Write()
	{
		_worldPacket.WriteBits((uint)Reason, 3);
		_worldPacket.FlushBits();
	}
}
