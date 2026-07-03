namespace HermesProxy.World.Server.Packets;

public class BuyBankSlot : ClientPacket
{
	public WowGuid128 Guid;

	public BuyBankSlot(WorldPacket packet)
		: base(packet)
	{
	}

	public override void Read()
	{
		Guid = _worldPacket.ReadPackedGuid128();
	}
}
