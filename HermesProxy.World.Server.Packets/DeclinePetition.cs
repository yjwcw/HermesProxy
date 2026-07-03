namespace HermesProxy.World.Server.Packets;

public class DeclinePetition : ClientPacket
{
	public WowGuid128 PetitionGUID;

	public DeclinePetition(WorldPacket packet)
		: base(packet)
	{
	}

	public override void Read()
	{
		PetitionGUID = _worldPacket.ReadPackedGuid128();
	}
}
