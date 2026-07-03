using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

public class DeleteChar : ServerPacket
{
	public byte Code;

	public DeleteChar()
		: base(Opcode.SMSG_DELETE_CHAR)
	{
	}

	public override void Write()
	{
		_worldPacket.WriteUInt8(Code);
	}
}
