namespace HermesProxy.World.Server.Packets;

public class UseItem : ClientPacket
{
	public byte PackSlot;

	public byte Slot;

	public WowGuid128 CastItem;

	public SpellCastRequest Cast;

	public UseItem(WorldPacket packet)
		: base(packet)
	{
		Cast = new SpellCastRequest();
	}

	public override void Read()
	{
		PackSlot = _worldPacket.ReadUInt8();
		Slot = _worldPacket.ReadUInt8();
		CastItem = _worldPacket.ReadPackedGuid128();
		Cast.Read(_worldPacket);
	}
}
