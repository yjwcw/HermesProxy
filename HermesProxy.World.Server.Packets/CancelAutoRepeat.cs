using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

public class CancelAutoRepeat : ServerPacket
{
	public WowGuid128 Guid;

	public CancelAutoRepeat()
		: base(Opcode.SMSG_CANCEL_AUTO_REPEAT)
	{
	}

	public override void Write()
	{
		_worldPacket.WritePackedGuid128(Guid);
	}
}
