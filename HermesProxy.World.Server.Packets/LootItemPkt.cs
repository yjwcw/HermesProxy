using System.Collections.Generic;

namespace HermesProxy.World.Server.Packets;

internal class LootItemPkt : ClientPacket
{
	public List<LootRequest> Loot = new List<LootRequest>();

	public LootItemPkt(WorldPacket packet)
		: base(packet)
	{
	}

	public override void Read()
	{
		uint num = _worldPacket.ReadUInt32();
		for (uint num2 = 0u; num2 < num; num2++)
		{
			LootRequest lootRequest = default(LootRequest);
			lootRequest.LootObj = _worldPacket.ReadPackedGuid128();
			lootRequest.LootListID = _worldPacket.ReadUInt8();
			LootRequest lootRequest2 = lootRequest;
			Loot.Add(lootRequest2);
		}
	}
}
