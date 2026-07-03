namespace HermesProxy.World.Server.Packets;

internal class LootRelease : ClientPacket
{
	public WowGuid128 Owner;

	public LootRelease(WorldPacket packet)
		: base(packet)
	{
	}

	public override void Read()
	{
		Owner = _worldPacket.ReadPackedGuid128();
	}
}
