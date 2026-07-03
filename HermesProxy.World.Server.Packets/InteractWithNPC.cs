namespace HermesProxy.World.Server.Packets;

public class InteractWithNPC : ClientPacket
{
	public WowGuid128 CreatureGUID;

	public InteractWithNPC(WorldPacket packet)
		: base(packet)
	{
	}

	public override void Read()
	{
		CreatureGUID = _worldPacket.ReadPackedGuid128();
	}
}
