namespace HermesProxy.World.Server.Packets;

public class OfferPetition : ClientPacket
{
	public uint UnkInt;

	public WowGuid128 TargetPlayer;

	public WowGuid128 ItemGUID;

	public OfferPetition(WorldPacket packet)
		: base(packet)
	{
	}

	public override void Read()
	{
		UnkInt = _worldPacket.ReadUInt32();
		ItemGUID = _worldPacket.ReadPackedGuid128();
		TargetPlayer = _worldPacket.ReadPackedGuid128();
	}
}
