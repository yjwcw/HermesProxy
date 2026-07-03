namespace HermesProxy.World.Server.Packets;

public class SetTradeItem : ClientPacket
{
	public byte TradeSlot;

	public byte PackSlot;

	public byte ItemSlotInPack;

	public SetTradeItem(WorldPacket packet)
		: base(packet)
	{
	}

	public override void Read()
	{
		TradeSlot = _worldPacket.ReadUInt8();
		PackSlot = _worldPacket.ReadUInt8();
		ItemSlotInPack = _worldPacket.ReadUInt8();
	}
}
