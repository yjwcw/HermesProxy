using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

public class SpiritHealerConfirm : ServerPacket
{
	public WowGuid128 Guid;

	public SpiritHealerConfirm()
		: base(Opcode.SMSG_SPIRIT_HEALER_CONFIRM)
	{
	}

	public override void Write()
	{
		_worldPacket.WritePackedGuid128(Guid);
	}
}
