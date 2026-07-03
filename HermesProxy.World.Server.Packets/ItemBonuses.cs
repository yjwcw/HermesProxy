using System.Collections.Generic;
using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

public class ItemBonuses
{
	public ItemContext Context;

	public List<uint> BonusListIDs = new List<uint>();

	public void Write(WorldPacket data)
	{
		data.WriteUInt8((byte)Context);
		data.WriteInt32(BonusListIDs.Count);
		foreach (uint bonusListID in BonusListIDs)
		{
			data.WriteUInt32(bonusListID);
		}
	}

	public void Read(WorldPacket data)
	{
		Context = (ItemContext)data.ReadUInt8();
		uint num = data.ReadUInt32();
		BonusListIDs = new List<uint>();
		for (uint num2 = 0u; num2 < num; num2++)
		{
			uint num3 = data.ReadUInt32();
			BonusListIDs.Add(num3);
		}
	}
}
