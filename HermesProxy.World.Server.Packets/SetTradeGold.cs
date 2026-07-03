namespace HermesProxy.World.Server.Packets;

public class SetTradeGold : ClientPacket
{
	public ulong Coinage;

	public SetTradeGold(WorldPacket packet)
		: base(packet)
	{
	}

	public override void Read()
	{
		Coinage = _worldPacket.ReadUInt64();
	}
}
