namespace HermesProxy.World.Server.Packets;

public class SignPetition : ClientPacket
{
	public WowGuid128 PetitionGUID;

	public byte Choice;

	public SignPetition(WorldPacket packet)
		: base(packet)
	{
	}

	public override void Read()
	{
		PetitionGUID = _worldPacket.ReadPackedGuid128();
		Choice = _worldPacket.ReadUInt8();
	}
}
