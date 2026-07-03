namespace HermesProxy.World.Server.Packets;

public class GuildInviteByName : ClientPacket
{
	public string Name;

	public uint ArenaTeamId;

	public GuildInviteByName(WorldPacket packet)
		: base(packet)
	{
	}

	public override void Read()
	{
		uint length = _worldPacket.ReadBits<uint>(9);
		bool num = _worldPacket.HasBit();
		Name = _worldPacket.ReadString(length);
		if (num)
		{
			ArenaTeamId = _worldPacket.ReadUInt32();
		}
	}
}
