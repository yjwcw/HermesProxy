using System.Collections.Generic;

namespace HermesProxy.World.Server.Packets;

internal class AuctionListBidderItems : ClientPacket
{
	public WowGuid128 Auctioneer;

	public uint Offset;

	public List<uint> AuctionItemIDs = new List<uint>();

	public AuctionListBidderItems(WorldPacket packet)
		: base(packet)
	{
	}

	public override void Read()
	{
		Auctioneer = _worldPacket.ReadPackedGuid128();
		Offset = _worldPacket.ReadUInt32();
		uint num = _worldPacket.ReadBits<uint>(7);
		_worldPacket.ResetBitPos();
		for (int i = 0; i < num; i++)
		{
			AuctionItemIDs[i] = _worldPacket.ReadUInt32();
		}
	}
}
