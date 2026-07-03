using System.Collections.Generic;
using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

public class AuctionListItemsResult : ServerPacket
{
	public List<AuctionItem> Items = new List<AuctionItem>();

	public int TotalItemsCount;

	public uint DesiredDelay = 300u;

	public bool OnlyUsable;

	public AuctionListItemsResult()
		: base(Opcode.SMSG_AUCTION_LIST_ITEMS_RESULT)
	{
	}

	public override void Write()
	{
		_worldPacket.WriteInt32(Items.Count);
		_worldPacket.WriteInt32(TotalItemsCount);
		_worldPacket.WriteUInt32(DesiredDelay);
		if (Items.Count > 0)
		{
			_worldPacket.WriteBool(OnlyUsable);
		}
		foreach (AuctionItem item in Items)
		{
			item.Write(_worldPacket);
		}
	}
}
