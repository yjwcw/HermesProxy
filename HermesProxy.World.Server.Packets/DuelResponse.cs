namespace HermesProxy.World.Server.Packets;

public class DuelResponse : ClientPacket
{
	public WowGuid128 ArbiterGUID;

	public bool Accepted;

	public bool Forfeited;

	public DuelResponse(WorldPacket packet)
		: base(packet)
	{
	}

	public override void Read()
	{
		ArbiterGUID = _worldPacket.ReadPackedGuid128();
		Accepted = _worldPacket.HasBit();
		Forfeited = _worldPacket.HasBit();
	}
}
