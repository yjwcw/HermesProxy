namespace HermesProxy.World.Server.Packets;

public class LogoutCancel : ClientPacket
{
	public LogoutCancel(WorldPacket packet)
		: base(packet)
	{
	}

	public override void Read()
	{
	}
}
