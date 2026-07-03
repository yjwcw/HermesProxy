namespace HermesProxy.World.Server.Packets;

internal class RequestBattlefieldStatus : ClientPacket
{
	public RequestBattlefieldStatus(WorldPacket packet)
		: base(packet)
	{
	}

	public override void Read()
	{
	}
}
