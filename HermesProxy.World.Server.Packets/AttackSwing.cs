namespace HermesProxy.World.Server.Packets;

public class AttackSwing : ClientPacket
{
	public WowGuid128 Victim;

	public AttackSwing(WorldPacket packet)
		: base(packet)
	{
	}

	public override void Read()
	{
		Victim = _worldPacket.ReadPackedGuid128();
	}
}
