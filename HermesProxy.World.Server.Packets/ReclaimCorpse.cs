namespace HermesProxy.World.Server.Packets;

public class ReclaimCorpse : ClientPacket
{
	public WowGuid128 CorpseGUID;

	public ReclaimCorpse(WorldPacket packet)
		: base(packet)
	{
	}

	public override void Read()
	{
		CorpseGUID = _worldPacket.ReadPackedGuid128();
	}
}
