using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

public class GuildEventRanksUpdated : ServerPacket
{
	public GuildEventRanksUpdated()
		: base(Opcode.SMSG_GUILD_EVENT_RANKS_UPDATED)
	{
	}

	public override void Write()
	{
	}
}
