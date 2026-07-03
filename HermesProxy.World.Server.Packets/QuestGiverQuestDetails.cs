using System;
using System.Collections.Generic;
using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

public class QuestGiverQuestDetails : ServerPacket
{
	public WowGuid128 QuestGiverGUID;

	public WowGuid128 InformUnit;

	public uint QuestID;

	public int QuestPackageID;

	public uint[] QuestFlags = new uint[2];

	public uint SuggestedPartyMembers;

	public QuestRewards Rewards = new QuestRewards();

	public List<QuestObjectiveSimple> Objectives = new List<QuestObjectiveSimple>();

	public QuestDescEmote[] DescEmotes = new QuestDescEmote[4];

	public List<uint> LearnSpells = new List<uint>();

	public uint PortraitTurnIn;

	public uint PortraitGiver;

	public uint PortraitGiverMount;

	public uint PortraitGiverModelSceneID;

	public int QuestStartItemID;

	public int QuestSessionBonus;

	public string PortraitGiverText = "";

	public string PortraitGiverName = "";

	public string PortraitTurnInText = "";

	public string PortraitTurnInName = "";

	public string QuestTitle = "";

	public string DescriptionText = "";

	public string LogDescription = "";

	public bool DisplayPopup;

	public bool StartCheat;

	public bool AutoLaunched;

	public QuestGiverQuestDetails()
		: base(Opcode.SMSG_QUEST_GIVER_QUEST_DETAILS)
	{
		for (int i = 0; i < 5; i++)
		{
			Rewards.FactionCapIn[i] = 7;
		}
	}

	public override void Write()
	{
		_worldPacket.WritePackedGuid128(QuestGiverGUID);
		_worldPacket.WritePackedGuid128(InformUnit);
		_worldPacket.WriteUInt32(QuestID);
		_worldPacket.WriteInt32(QuestPackageID);
		_worldPacket.WriteUInt32(PortraitGiver);
		_worldPacket.WriteUInt32(PortraitGiverMount);
		_worldPacket.WriteUInt32(PortraitGiverModelSceneID);
		_worldPacket.WriteUInt32(PortraitTurnIn);
		_worldPacket.WriteUInt32(QuestFlags[0]);
		_worldPacket.WriteUInt32(QuestFlags[1]);
		_worldPacket.WriteUInt32(SuggestedPartyMembers);
		_worldPacket.WriteInt32(LearnSpells.Count);
		_worldPacket.WriteInt32(DescEmotes.Length);
		_worldPacket.WriteInt32(Objectives.Count);
		_worldPacket.WriteInt32(QuestStartItemID);
		_worldPacket.WriteInt32(QuestSessionBonus);
		foreach (uint learnSpell in LearnSpells)
		{
			_worldPacket.WriteUInt32(learnSpell);
		}
		QuestDescEmote[] descEmotes = DescEmotes;
		for (int i = 0; i < descEmotes.Length; i++)
		{
			QuestDescEmote questDescEmote = descEmotes[i];
			_worldPacket.WriteUInt32(questDescEmote.Type);
			_worldPacket.WriteUInt32(questDescEmote.Delay);
		}
		foreach (QuestObjectiveSimple objective in Objectives)
		{
			_worldPacket.WriteUInt32(objective.Id);
			_worldPacket.WriteInt32(objective.ObjectID);
			_worldPacket.WriteInt32(objective.Amount);
			_worldPacket.WriteUInt8(objective.Type);
		}
		_worldPacket.WriteBits(QuestTitle.GetByteCount(), 9);
		_worldPacket.WriteBits(DescriptionText.GetByteCount(), 12);
		_worldPacket.WriteBits(LogDescription.GetByteCount(), 12);
		_worldPacket.WriteBits(PortraitGiverText.GetByteCount(), 10);
		_worldPacket.WriteBits(PortraitGiverName.GetByteCount(), 8);
		_worldPacket.WriteBits(PortraitTurnInText.GetByteCount(), 10);
		_worldPacket.WriteBits(PortraitTurnInName.GetByteCount(), 8);
		_worldPacket.WriteBit(AutoLaunched);
		_worldPacket.WriteBit(bit: false);
		_worldPacket.WriteBit(StartCheat);
		_worldPacket.WriteBit(DisplayPopup);
		_worldPacket.FlushBits();
		Rewards.Write(_worldPacket);
		_worldPacket.WriteString(QuestTitle);
		_worldPacket.WriteString(DescriptionText);
		_worldPacket.WriteString(LogDescription);
		_worldPacket.WriteString(PortraitGiverText);
		_worldPacket.WriteString(PortraitGiverName);
		_worldPacket.WriteString(PortraitTurnInText);
		_worldPacket.WriteString(PortraitTurnInName);
	}
}
