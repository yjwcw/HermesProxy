using System.Collections.Generic;

namespace HermesProxy.World.Server.Packets;

internal class LootMasterGive : ClientPacket
{
	public WowGuid128 TargetGUID;

	public List<LootRequest> Loot = new List<LootRequest>();

	public LootMasterGive(WorldPacket packet)
		: base(packet)
	{
	}

	public override void Read()
	{
		uint num = _worldPacket.ReadUInt32();
		TargetGUID = _worldPacket.ReadPackedGuid128();
		for (int i = 0; i < num; i++)
		{
			LootRequest lootRequest = default(LootRequest);
			lootRequest.LootObj = _worldPacket.ReadPackedGuid128();
			lootRequest.LootListID = _worldPacket.ReadUInt8();
			Loot.Add(lootRequest);
		}
	}
}
