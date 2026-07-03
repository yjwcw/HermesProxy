using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

public class CreateChar : ServerPacket
{
	public byte Code;

	public WowGuid128 Guid;

	public CreateChar()
		: base(Opcode.SMSG_CREATE_CHAR)
	{
	}

	public override void Write()
	{
		_worldPacket.WriteUInt8(Code);
		_worldPacket.WritePackedGuid128(Guid);
	}
}
