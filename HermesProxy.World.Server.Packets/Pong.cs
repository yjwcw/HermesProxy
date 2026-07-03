using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

internal class Pong : ServerPacket
{
	private uint Serial;

	public Pong(uint serial)
		: base(Opcode.SMSG_PONG)
	{
		Serial = serial;
	}

	public override void Write()
	{
		_worldPacket.WriteUInt32(Serial);
	}
}
