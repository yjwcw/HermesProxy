using System.Collections.Generic;

namespace HermesProxy.World.Server.Packets;

public class MailAttachedItem
{
	public byte Position;

	public int AttachID;

	public ItemInstance Item = new ItemInstance();

	public uint Count;

	public int Charges;

	public uint MaxDurability;

	public uint Durability;

	public bool Unlocked;

	public List<ItemEnchantData> Enchants = new List<ItemEnchantData>();

	public List<ItemGemData> Gems = new List<ItemGemData>();

	public void Write(WorldPacket data)
	{
		data.WriteUInt8(Position);
		data.WriteInt32(AttachID);
		data.WriteUInt32(Count);
		data.WriteInt32(Charges);
		data.WriteUInt32(MaxDurability);
		data.WriteUInt32(Durability);
		Item.Write(data);
		data.WriteBits(Enchants.Count, 4);
		data.WriteBits(Gems.Count, 2);
		data.WriteBit(Unlocked);
		data.FlushBits();
		foreach (ItemGemData gem in Gems)
		{
			gem.Write(data);
		}
		foreach (ItemEnchantData enchant in Enchants)
		{
			enchant.Write(data);
		}
	}
}
