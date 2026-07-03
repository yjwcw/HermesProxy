using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

public class BinderConfirm : ServerPacket
{
	public WowGuid128 Guid;

	public BinderConfirm()
		: base(Opcode.SMSG_BINDER_CONFIRM)
	{
	}

	public override void Write()
	{
		_worldPacket.WritePackedGuid128(Guid);
	}
}
