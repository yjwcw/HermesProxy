namespace HermesProxy.World.Server.Packets;

public class PetCastSpell : ClientPacket
{
	public WowGuid128 PetGUID;

	public SpellCastRequest Cast;

	public PetCastSpell(WorldPacket packet)
		: base(packet)
	{
		Cast = new SpellCastRequest();
	}

	public override void Read()
	{
		PetGUID = _worldPacket.ReadPackedGuid128();
		Cast.Read(_worldPacket);
	}
}
