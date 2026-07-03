using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

public class GossipComplete : ServerPacket
{
	public bool SuppressSound;

	public GossipComplete()
		: base(Opcode.SMSG_GOSSIP_COMPLETE)
	{
	}

	public override void Write()
	{
		if (ModernVersion.AddedInVersion(9, 2, 0, 1, 14, 2, 2, 5, 3))
		{
			_worldPacket.WriteBit(SuppressSound);
			_worldPacket.FlushBits();
		}
	}
}
