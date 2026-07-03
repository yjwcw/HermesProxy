namespace HermesProxy.World.Server.Packets;

public class WrapItem : ClientPacket
{
	public byte GiftBag;

	public byte GiftSlot;

	public byte ItemBag;

	public byte ItemSlot;

	public WrapItem(WorldPacket packet)
		: base(packet)
	{
	}

	public override void Read()
	{
		_worldPacket.ReadUInt8();
		GiftBag = _worldPacket.ReadUInt8();
		GiftSlot = _worldPacket.ReadUInt8();
		ItemBag = _worldPacket.ReadUInt8();
		ItemSlot = _worldPacket.ReadUInt8();
	}
}
