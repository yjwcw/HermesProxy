namespace HermesProxy.World.Server.Packets;

internal class PVPLogDataRequest : ClientPacket
{
	public PVPLogDataRequest(WorldPacket packet)
		: base(packet)
	{
	}

	public override void Read()
	{
	}
}
