using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

internal class ReadyCheckResponse : ServerPacket
{
	public WowGuid128 PartyGUID;

	public WowGuid128 Player;

	public bool IsReady;

	public ReadyCheckResponse()
		: base(Opcode.SMSG_READY_CHECK_RESPONSE)
	{
	}

	public override void Write()
	{
		_worldPacket.WritePackedGuid128(PartyGUID);
		_worldPacket.WritePackedGuid128(Player);
		_worldPacket.WriteBit(IsReady);
		_worldPacket.FlushBits();
	}
}
