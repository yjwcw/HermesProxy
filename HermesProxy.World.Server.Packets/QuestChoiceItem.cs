namespace HermesProxy.World.Server.Packets;

public class QuestChoiceItem
{
	public byte LootItemType;

	public ItemInstance Item = new ItemInstance();

	public uint Quantity;

	public void Read(WorldPacket data)
	{
		data.ResetBitPos();
		LootItemType = data.ReadBits<byte>(2);
		Item.Read(data);
		Quantity = data.ReadUInt32();
	}

	public void Write(WorldPacket data)
	{
		data.WriteBits(LootItemType, 2);
		Item.Write(data);
		data.WriteUInt32(Quantity);
	}
}
