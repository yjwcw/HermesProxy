using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

internal class GroupUninvite : ServerPacket
{
	public GroupUninvite()
		: base(Opcode.SMSG_GROUP_UNINVITE)
	{
	}

	public override void Write()
	{
	}
}
