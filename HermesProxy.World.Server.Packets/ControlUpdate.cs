using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

public class ControlUpdate : ServerPacket
{
	public WowGuid128 Guid;

	public bool HasControl;

	public ControlUpdate()
		: base(Opcode.SMSG_CONTROL_UPDATE)
	{
	}

	public override void Write()
	{
		_worldPacket.WritePackedGuid128(Guid);
		_worldPacket.WriteBit(HasControl);
		_worldPacket.FlushBits();
	}
}
