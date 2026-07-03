using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

internal class RaidGroupOnly : ServerPacket
{
	public int Delay;

	public RaidGroupReason Reason;

	public RaidGroupOnly()
		: base(Opcode.SMSG_RAID_GROUP_ONLY)
	{
	}

	public override void Write()
	{
		_worldPacket.WriteInt32(Delay);
		_worldPacket.WriteUInt32((uint)Reason);
	}
}
