namespace HermesProxy.World.Server.Packets;

public class QueryPetition : ClientPacket
{
	public WowGuid128 ItemGUID;

	public uint PetitionID;

	public QueryPetition(WorldPacket packet)
		: base(packet)
	{
	}

	public override void Read()
	{
		PetitionID = _worldPacket.ReadUInt32();
		ItemGUID = _worldPacket.ReadPackedGuid128();
	}
}
