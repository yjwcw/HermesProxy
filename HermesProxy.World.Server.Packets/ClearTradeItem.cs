namespace HermesProxy.World.Server.Packets;

public class ClearTradeItem : ClientPacket
{
	public byte TradeSlot;

	public ClearTradeItem(WorldPacket packet)
		: base(packet)
	{
	}

	public override void Read()
	{
		TradeSlot = _worldPacket.ReadUInt8();
	}
}
