namespace HermesProxy.World.Server.Packets;

internal class TrainerBuySpell : ClientPacket
{
	public WowGuid128 TrainerGUID;

	public uint TrainerID;

	public uint SpellID;

	public TrainerBuySpell(WorldPacket packet)
		: base(packet)
	{
	}

	public override void Read()
	{
		TrainerGUID = _worldPacket.ReadPackedGuid128();
		TrainerID = _worldPacket.ReadUInt32();
		SpellID = _worldPacket.ReadUInt32();
	}
}
