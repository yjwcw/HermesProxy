using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

internal class ReadyCheckCompleted : ServerPacket
{
	public sbyte PartyIndex;

	public WowGuid128 PartyGUID;

	public ReadyCheckCompleted()
		: base(Opcode.SMSG_READY_CHECK_COMPLETED)
	{
	}

	public override void Write()
	{
		_worldPacket.WriteInt8(PartyIndex);
		_worldPacket.WritePackedGuid128(PartyGUID);
	}
}
