namespace HermesProxy.World.Server.Packets;

public class SetActiveMover : ClientPacket
{
	public WowGuid128 MoverGUID;

	public SetActiveMover(WorldPacket packet)
		: base(packet)
	{
	}

	public override void Read()
	{
		MoverGUID = _worldPacket.ReadPackedGuid128();
	}
}
