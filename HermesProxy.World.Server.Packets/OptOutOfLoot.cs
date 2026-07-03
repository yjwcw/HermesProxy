namespace HermesProxy.World.Server.Packets;

internal class OptOutOfLoot : ClientPacket
{
	public bool PassOnLoot;

	public OptOutOfLoot(WorldPacket packet)
		: base(packet)
	{
	}

	public override void Read()
	{
		PassOnLoot = _worldPacket.HasBit();
	}
}
