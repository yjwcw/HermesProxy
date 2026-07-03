using System.Collections.Generic;

namespace HermesProxy.World.Server.Packets;

public class InspectItemData
{
	public WowGuid128 CreatorGUID = WowGuid128.Empty;

	public ItemInstance Item = new ItemInstance();

	public byte Index;

	public bool Usable;

	public List<InspectEnchantData> Enchants = new List<InspectEnchantData>();

	public List<ItemGemData> Gems = new List<ItemGemData>();

	public List<int> AzeritePowers = new List<int>();

	public List<AzeriteEssenceData> AzeriteEssences = new List<AzeriteEssenceData>();

	public void Write(WorldPacket data)
	{
		data.WritePackedGuid128(CreatorGUID);
		data.WriteUInt8(Index);
		data.WriteInt32(AzeritePowers.Count);
		data.WriteInt32(AzeriteEssences.Count);
		foreach (int azeritePower in AzeritePowers)
		{
			data.WriteInt32(azeritePower);
		}
		Item.Write(data);
		data.WriteBit(Usable);
		data.WriteBits(Enchants.Count, 4);
		data.WriteBits(Gems.Count, 2);
		data.FlushBits();
		foreach (AzeriteEssenceData azeriteEssence in AzeriteEssences)
		{
			azeriteEssence.Write(data);
		}
		foreach (InspectEnchantData enchant in Enchants)
		{
			enchant.Write(data);
		}
		foreach (ItemGemData gem in Gems)
		{
			gem.Write(data);
		}
	}
}
