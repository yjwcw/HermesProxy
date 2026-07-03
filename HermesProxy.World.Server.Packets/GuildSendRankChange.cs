using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

public class GuildSendRankChange : ServerPacket
{
	public WowGuid128 Other;

	public WowGuid128 Officer;

	public bool Promote;

	public uint RankID;

	public GuildSendRankChange()
		: base(Opcode.SMSG_GUILD_SEND_RANK_CHANGE)
	{
	}

	public override void Write()
	{
		_worldPacket.WritePackedGuid128(Officer);
		_worldPacket.WritePackedGuid128(Other);
		_worldPacket.WriteUInt32(RankID);
		_worldPacket.WriteBit(Promote);
		_worldPacket.FlushBits();
	}
}
