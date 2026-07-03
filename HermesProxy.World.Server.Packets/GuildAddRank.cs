namespace HermesProxy.World.Server.Packets;

public class GuildAddRank : ClientPacket
{
	public string Name;

	public int RankOrder;

	public GuildAddRank(WorldPacket packet)
		: base(packet)
	{
	}

	public override void Read()
	{
		uint length = _worldPacket.ReadBits<uint>(7);
		_worldPacket.ResetBitPos();
		RankOrder = _worldPacket.ReadInt32();
		Name = _worldPacket.ReadString(length);
	}
}
