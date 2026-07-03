using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

internal class PvPCredit : ServerPacket
{
	public int OriginalHonor;

	public int Honor;

	public WowGuid128 Target;

	public uint Rank;

	public PvPCredit()
		: base(Opcode.SMSG_PVP_CREDIT)
	{
	}

	public override void Write()
	{
		_worldPacket.WriteInt32(OriginalHonor);
		_worldPacket.WriteInt32(Honor);
		_worldPacket.WritePackedGuid128(Target);
		_worldPacket.WriteUInt32(Rank);
	}
}
