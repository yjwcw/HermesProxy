using System.Collections.Generic;

namespace HermesProxy.World.Server.Packets;

public class TreasureLootList
{
	public List<TreasureItem> Items = new List<TreasureItem>();

	public void Write(WorldPacket data)
	{
		data.WriteInt32(Items.Count);
		foreach (TreasureItem item in Items)
		{
			item.Write(data);
		}
	}
}
