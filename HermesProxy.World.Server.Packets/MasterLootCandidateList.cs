using System.Collections.Generic;
using Framework.Constants;
using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

internal class MasterLootCandidateList : ServerPacket
{
	public WowGuid128 LootObj;

	public List<WowGuid128> Players = new List<WowGuid128>();

	public MasterLootCandidateList()
		: base(Opcode.SMSG_LOOT_MASTER_LIST, ConnectionType.Instance)
	{
	}

	public override void Write()
	{
		_worldPacket.WritePackedGuid128(LootObj);
		_worldPacket.WriteInt32(Players.Count);
		foreach (WowGuid128 player in Players)
		{
			_worldPacket.WritePackedGuid128(player);
		}
	}
}
