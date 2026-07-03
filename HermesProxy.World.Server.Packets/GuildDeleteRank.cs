namespace HermesProxy.World.Server.Packets;

public class GuildDeleteRank : ClientPacket
{
	public int RankOrder;

	public GuildDeleteRank(WorldPacket packet)
		: base(packet)
	{
	}

	public override void Read()
	{
		RankOrder = _worldPacket.ReadInt32();
	}
}
