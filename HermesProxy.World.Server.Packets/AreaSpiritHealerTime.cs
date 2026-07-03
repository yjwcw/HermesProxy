using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

public class AreaSpiritHealerTime : ServerPacket
{
	public WowGuid128 HealerGuid;

	public uint TimeLeft;

	public AreaSpiritHealerTime()
		: base(Opcode.SMSG_AREA_SPIRIT_HEALER_TIME)
	{
	}

	public override void Write()
	{
		_worldPacket.WritePackedGuid128(HealerGuid);
		_worldPacket.WriteUInt32(TimeLeft);
	}
}
