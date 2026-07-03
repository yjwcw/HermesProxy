namespace HermesProxy.World.Server.Packets;

public class ItemInstance
{
	public uint ItemID;

	public uint RandomPropertiesSeed;

	public uint RandomPropertiesID;

	public ItemBonuses ItemBonus;

	public ItemModList Modifications = new ItemModList();

	public void Write(WorldPacket data)
	{
		data.WriteUInt32(ItemID);
		data.WriteUInt32(RandomPropertiesSeed);
		data.WriteUInt32(RandomPropertiesID);
		data.WriteBit(ItemBonus != null);
		data.FlushBits();
		Modifications.Write(data);
		if (ItemBonus != null)
		{
			ItemBonus.Write(data);
		}
	}

	public void Read(WorldPacket data)
	{
		ItemID = data.ReadUInt32();
		RandomPropertiesSeed = data.ReadUInt32();
		RandomPropertiesID = data.ReadUInt32();
		if (data.HasBit())
		{
			ItemBonus = new ItemBonuses();
		}
		data.ResetBitPos();
		Modifications.Read(data);
		if (ItemBonus != null)
		{
			ItemBonus.Read(data);
		}
	}
}
