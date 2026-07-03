using System.Collections.Generic;

namespace HermesProxy.World.Server.Packets;

public class ItemModList
{
	public Array<ItemMod> Values = new Array<ItemMod>(38);

	public void Read(WorldPacket data)
	{
		uint num = data.ReadBits<uint>(6);
		data.ResetBitPos();
		for (int i = 0; i < num; i++)
		{
			ItemMod itemMod = new ItemMod();
			itemMod.Read(data);
			Values[i] = itemMod;
		}
	}

	public void Write(WorldPacket data)
	{
		data.WriteBits(Values.Count, 6);
		data.FlushBits();
		foreach (ItemMod value in Values)
		{
			value.Write(data);
		}
	}
}
