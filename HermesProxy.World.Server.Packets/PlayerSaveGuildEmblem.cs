using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

public class PlayerSaveGuildEmblem : ServerPacket
{
	public GuildEmblemError Error;

	public PlayerSaveGuildEmblem()
		: base(Opcode.SMSG_PLAYER_SAVE_GUILD_EMBLEM)
	{
	}

	public override void Write()
	{
		_worldPacket.WriteUInt32((uint)Error);
	}
}
