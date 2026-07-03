using System.Collections.Generic;
using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

public class AuctionListMyItemsResult : ServerPacket
{
	public List<AuctionItem> Items = new List<AuctionItem>();

	public int TotalItemsCount;

	public uint DesiredDelay = 300u;

	public AuctionListMyItemsResult(Opcode opcode)
		: base(opcode)
	{
	}

	public override void Write()
	{
		_worldPacket.WriteInt32(Items.Count);
		_worldPacket.WriteInt32(TotalItemsCount);
		_worldPacket.WriteUInt32(DesiredDelay);
		foreach (AuctionItem item in Items)
		{
			item.Write(_worldPacket);
		}
	}
}
