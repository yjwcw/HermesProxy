namespace HermesProxy.World.Server.Packets;

public class GameObjUse : ClientPacket
{
	public WowGuid128 Guid;

	public GameObjUse(WorldPacket packet)
		: base(packet)
	{
	}

	public override void Read()
	{
		Guid = _worldPacket.ReadPackedGuid128();
	}
}
