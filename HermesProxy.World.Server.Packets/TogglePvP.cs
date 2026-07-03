namespace HermesProxy.World.Server.Packets;

internal class TogglePvP : ClientPacket
{
	public TogglePvP(WorldPacket packet)
		: base(packet)
	{
	}

	public override void Read()
	{
	}
}
