namespace HermesProxy.World.Server.Packets;

internal class BuyStableSlot : ClientPacket
{
	public WowGuid128 StableMaster;

	public BuyStableSlot(WorldPacket packet)
		: base(packet)
	{
	}

	public override void Read()
	{
		StableMaster = _worldPacket.ReadPackedGuid128();
	}
}
