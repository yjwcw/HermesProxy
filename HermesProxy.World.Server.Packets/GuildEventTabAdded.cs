using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

public class GuildEventTabAdded : ServerPacket
{
	public GuildEventTabAdded()
		: base(Opcode.SMSG_GUILD_EVENT_TAB_ADDED)
	{
	}

	public override void Write()
	{
	}
}
