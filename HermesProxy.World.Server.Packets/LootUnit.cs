namespace HermesProxy.World.Server.Packets;

internal class LootUnit : ClientPacket
{
	public WowGuid128 Unit;

	public LootUnit(WorldPacket packet)
		: base(packet)
	{
	}

	public override void Read()
	{
		Unit = _worldPacket.ReadPackedGuid128();
	}
}
