namespace HermesProxy.World.Server.Packets;

public class CanDuel : ClientPacket
{
	public WowGuid128 TargetGUID;

	public CanDuel(WorldPacket packet)
		: base(packet)
	{
	}

	public override void Read()
	{
		TargetGUID = _worldPacket.ReadPackedGuid128();
	}
}
