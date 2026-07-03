namespace HermesProxy.World.Server.Packets;

public class CastSpell : ClientPacket
{
	public SpellCastRequest Cast;

	public CastSpell(WorldPacket packet)
		: base(packet)
	{
		Cast = new SpellCastRequest();
	}

	public override void Read()
	{
		Cast.Read(_worldPacket);
	}
}
