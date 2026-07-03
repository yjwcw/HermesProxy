using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

internal class InstanceSaveCreated : ServerPacket
{
	public bool Gm;

	public InstanceSaveCreated()
		: base(Opcode.SMSG_INSTANCE_SAVE_CREATED)
	{
	}

	public override void Write()
	{
		_worldPacket.WriteBit(Gm);
		_worldPacket.FlushBits();
	}
}
