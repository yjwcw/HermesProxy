namespace HermesProxy.World.Server.Packets;

internal class PetAbandon : ClientPacket
{
	public WowGuid128 PetGUID;

	public PetAbandon(WorldPacket packet)
		: base(packet)
	{
	}

	public override void Read()
	{
		PetGUID = _worldPacket.ReadPackedGuid128();
	}
}
