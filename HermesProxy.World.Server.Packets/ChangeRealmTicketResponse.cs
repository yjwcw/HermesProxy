using Framework.IO;
using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

internal class ChangeRealmTicketResponse : ServerPacket
{
	public uint Token;

	public bool Allow = true;

	public ByteBuffer Ticket = new ByteBuffer();

	public ChangeRealmTicketResponse()
		: base(Opcode.SMSG_CHANGE_REALM_TICKET_RESPONSE)
	{
	}

	public override void Write()
	{
		_worldPacket.WriteUInt32(Token);
		_worldPacket.WriteBit(Allow);
		_worldPacket.WriteUInt32(Ticket.GetSize());
		_worldPacket.WriteBytes(Ticket);
	}
}
