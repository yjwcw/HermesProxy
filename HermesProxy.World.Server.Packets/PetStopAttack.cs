namespace HermesProxy.World.Server.Packets;

internal class PetStopAttack : ClientPacket
{
	public WowGuid128 PetGUID;

	public PetStopAttack(WorldPacket packet)
		: base(packet)
	{
	}

	public override void Read()
	{
		PetGUID = _worldPacket.ReadPackedGuid128();
	}
}
