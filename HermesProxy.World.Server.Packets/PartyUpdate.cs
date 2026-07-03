using System.Collections.Generic;
using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

public class PartyUpdate : ServerPacket
{
	public GroupFlags PartyFlags;

	public byte PartyIndex;

	public GroupType PartyType;

	public WowGuid128 PartyGUID;

	public WowGuid128 LeaderGUID;

	public int MyIndex;

	public int SequenceNum;

	public List<PartyPlayerInfo> PlayerList = new List<PartyPlayerInfo>();

	public PartyLFGInfo LfgInfos;

	public PartyLootSettings LootSettings;

	public PartyDifficultySettings DifficultySettings;

	public PartyUpdate()
		: base(Opcode.SMSG_PARTY_UPDATE)
	{
	}

	public override void Write()
	{
		_worldPacket.WriteUInt16((ushort)PartyFlags);
		_worldPacket.WriteUInt8(PartyIndex);
		_worldPacket.WriteUInt8((byte)PartyType);
		_worldPacket.WriteInt32(MyIndex);
		_worldPacket.WritePackedGuid128(PartyGUID);
		_worldPacket.WriteInt32(SequenceNum);
		_worldPacket.WritePackedGuid128(LeaderGUID);
		_worldPacket.WriteInt32(PlayerList.Count);
		_worldPacket.WriteBit(LfgInfos != null);
		_worldPacket.WriteBit(LootSettings != null);
		_worldPacket.WriteBit(DifficultySettings != null);
		_worldPacket.FlushBits();
		foreach (PartyPlayerInfo player in PlayerList)
		{
			player.Write(_worldPacket);
		}
		if (LootSettings != null)
		{
			LootSettings.Write(_worldPacket);
		}
		if (DifficultySettings != null)
		{
			DifficultySettings.Write(_worldPacket);
		}
		if (LfgInfos != null)
		{
			LfgInfos.Write(_worldPacket);
		}
	}
}
