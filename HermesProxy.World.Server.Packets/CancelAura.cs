namespace HermesProxy.World.Server.Packets;

internal class CancelAura : ClientPacket
{
	public uint SpellID;

	public WowGuid128 CasterGUID;

	public CancelAura(WorldPacket packet)
		: base(packet)
	{
	}

	public override void Read()
	{
		SpellID = _worldPacket.ReadUInt32();
		CasterGUID = _worldPacket.ReadPackedGuid128();
	}
}
