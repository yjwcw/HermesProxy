namespace HermesProxy.World.Server.Packets;

internal class PetCancelAura : ClientPacket
{
	public WowGuid128 PetGUID;

	public uint SpellID;

	public PetCancelAura(WorldPacket packet)
		: base(packet)
	{
	}

	public override void Read()
	{
		PetGUID = _worldPacket.ReadPackedGuid128();
		SpellID = _worldPacket.ReadUInt32();
	}
}
