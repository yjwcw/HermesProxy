using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

public class BattlefieldStatusFailed : ServerPacket
{
	public RideTicket Ticket = new RideTicket();

	public byte Unk;

	public ulong BattlefieldListId;

	public WowGuid128 ClientID = WowGuid128.Empty;

	public int Reason;

	public BattlefieldStatusFailed()
		: base(Opcode.SMSG_BATTLEFIELD_STATUS_FAILED)
	{
	}

	public override void Write()
	{
		Ticket.Write(_worldPacket);
		ulong data = BattlefieldListId | 0x1F10000000000000uL;
		_worldPacket.WriteUInt64(data);
		_worldPacket.WriteInt32(Reason);
		_worldPacket.WritePackedGuid128(ClientID);
	}
}
