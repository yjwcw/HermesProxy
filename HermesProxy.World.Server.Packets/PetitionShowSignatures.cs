namespace HermesProxy.World.Server.Packets;

public class PetitionShowSignatures : ClientPacket
{
	public WowGuid128 Item;

	public PetitionShowSignatures(WorldPacket packet)
		: base(packet)
	{
	}

	public override void Read()
	{
		Item = _worldPacket.ReadPackedGuid128();
	}
}
