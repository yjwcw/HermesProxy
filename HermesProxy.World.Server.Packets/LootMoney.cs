namespace HermesProxy.World.Server.Packets;

internal class LootMoney : ClientPacket
{
	public LootMoney(WorldPacket packet)
		: base(packet)
	{
	}

	public override void Read()
	{
	}
}
