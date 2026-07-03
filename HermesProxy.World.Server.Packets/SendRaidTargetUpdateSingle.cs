using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

internal class SendRaidTargetUpdateSingle : ServerPacket
{
	public sbyte PartyIndex;

	public sbyte Symbol;

	public WowGuid128 Target;

	public WowGuid128 ChangedBy;

	public SendRaidTargetUpdateSingle()
		: base(Opcode.SMSG_SEND_RAID_TARGET_UPDATE_SINGLE)
	{
	}

	public override void Write()
	{
		_worldPacket.WriteInt8(PartyIndex);
		_worldPacket.WriteInt8(Symbol);
		_worldPacket.WritePackedGuid128(Target);
		_worldPacket.WritePackedGuid128(ChangedBy);
	}
}
