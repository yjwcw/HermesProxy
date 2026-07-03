namespace HermesProxy.World.Server.Packets;

public class CancelCast : ClientPacket
{
	public uint SpellID;

	public WowGuid128 CastID;

	public CancelCast(WorldPacket packet)
		: base(packet)
	{
	}

	public override void Read()
	{
		CastID = _worldPacket.ReadPackedGuid128();
		SpellID = _worldPacket.ReadUInt32();
	}
}
