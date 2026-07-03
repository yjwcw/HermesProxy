namespace HermesProxy.World.Server.Packets;

internal class CancelAutoRepeatSpell : ClientPacket
{
	public CancelAutoRepeatSpell(WorldPacket packet)
		: base(packet)
	{
	}

	public override void Read()
	{
	}
}
