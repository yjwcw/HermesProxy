namespace HermesProxy.World.Server.Packets;

public class RepopRequest : ClientPacket
{
	public bool CheckInstance;

	public RepopRequest(WorldPacket packet)
		: base(packet)
	{
	}

	public override void Read()
	{
		CheckInstance = _worldPacket.HasBit();
	}
}
