using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

public class ZoneUnderAttack : ServerPacket
{
	public int AreaID;

	public ZoneUnderAttack()
		: base(Opcode.SMSG_ZONE_UNDER_ATTACK)
	{
	}

	public override void Write()
	{
		_worldPacket.WriteInt32(AreaID);
	}
}
