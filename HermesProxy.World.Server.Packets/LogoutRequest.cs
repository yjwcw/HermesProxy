namespace HermesProxy.World.Server.Packets;

public class LogoutRequest : ClientPacket
{
	public bool IdleLogout;

	public LogoutRequest(WorldPacket packet)
		: base(packet)
	{
	}

	public override void Read()
	{
		IdleLogout = _worldPacket.HasBit();
	}
}
