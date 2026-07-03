namespace HermesProxy.World.Server.Packets;

public class WorldPortResponse : ClientPacket
{
	public WorldPortResponse(WorldPacket packet)
		: base(packet)
	{
	}

	public override void Read()
	{
	}
}
