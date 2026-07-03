using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

internal class ReadyCheckStarted : ServerPacket
{
	public sbyte PartyIndex;

	public WowGuid128 PartyGUID;

	public WowGuid128 InitiatorGUID;

	public ulong Duration = 35000uL;

	public ReadyCheckStarted()
		: base(Opcode.SMSG_READY_CHECK_STARTED)
	{
	}

	public override void Write()
	{
		_worldPacket.WriteInt8(PartyIndex);
		_worldPacket.WritePackedGuid128(PartyGUID);
		_worldPacket.WritePackedGuid128(InitiatorGUID);
		_worldPacket.WriteUInt64(Duration);
	}
}
