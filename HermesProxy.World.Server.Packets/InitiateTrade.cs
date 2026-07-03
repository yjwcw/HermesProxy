namespace HermesProxy.World.Server.Packets;

public class InitiateTrade : ClientPacket
{
	public WowGuid128 Guid;

	public InitiateTrade(WorldPacket packet)
		: base(packet)
	{
	}

	public override void Read()
	{
		Guid = _worldPacket.ReadPackedGuid128();
	}
}
