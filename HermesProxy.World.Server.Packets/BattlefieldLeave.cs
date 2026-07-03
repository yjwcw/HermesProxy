namespace HermesProxy.World.Server.Packets;

internal class BattlefieldLeave : ClientPacket
{
	public BattlefieldLeave(WorldPacket packet)
		: base(packet)
	{
	}

	public override void Read()
	{
	}
}
