using System;
using System.Collections.Generic;
using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

public class QuestGiverRequestItems : ServerPacket
{
	public WowGuid128 QuestGiverGUID;

	public uint QuestGiverCreatureID;

	public uint QuestID;

	public uint CompEmoteDelay;

	public uint CompEmoteType;

	public bool AutoLaunched;

	public uint SuggestPartyMembers;

	public int MoneyToGet;

	public List<QuestObjectiveCollect> Collect = new List<QuestObjectiveCollect>();

	public List<QuestCurrency> Currency = new List<QuestCurrency>();

	public uint StatusFlags;

	public uint[] QuestFlags = new uint[2];

	public string QuestTitle = "";

	public string CompletionText = "";

	public QuestGiverRequestItems()
		: base(Opcode.SMSG_QUEST_GIVER_REQUEST_ITEMS)
	{
	}

	public override void Write()
	{
		_worldPacket.WritePackedGuid128(QuestGiverGUID);
		_worldPacket.WriteUInt32(QuestGiverCreatureID);
		_worldPacket.WriteUInt32(QuestID);
		_worldPacket.WriteUInt32(CompEmoteDelay);
		_worldPacket.WriteUInt32(CompEmoteType);
		_worldPacket.WriteUInt32(QuestFlags[0]);
		_worldPacket.WriteUInt32(QuestFlags[1]);
		_worldPacket.WriteUInt32(SuggestPartyMembers);
		_worldPacket.WriteInt32(MoneyToGet);
		_worldPacket.WriteInt32(Collect.Count);
		_worldPacket.WriteInt32(Currency.Count);
		_worldPacket.WriteUInt32(StatusFlags);
		foreach (QuestObjectiveCollect item in Collect)
		{
			_worldPacket.WriteUInt32(item.ObjectID);
			_worldPacket.WriteUInt32(item.Amount);
			_worldPacket.WriteUInt32(item.Flags);
		}
		foreach (QuestCurrency item2 in Currency)
		{
			_worldPacket.WriteUInt32(item2.CurrencyID);
			_worldPacket.WriteInt32(item2.Amount);
		}
		_worldPacket.WriteBit(AutoLaunched);
		_worldPacket.FlushBits();
		_worldPacket.WriteBits(QuestTitle.GetByteCount(), 9);
		_worldPacket.WriteBits(CompletionText.GetByteCount(), 12);
		_worldPacket.WriteString(QuestTitle);
		_worldPacket.WriteString(CompletionText);
	}
}
