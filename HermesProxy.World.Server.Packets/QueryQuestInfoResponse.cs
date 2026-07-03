using System;
using Framework.Constants;
using HermesProxy.World.Enums;
using HermesProxy.World.Objects;

namespace HermesProxy.World.Server.Packets;

public class QueryQuestInfoResponse : ServerPacket
{
	public bool Allow;

	public QuestTemplate Info;

	public uint QuestID;

	public QueryQuestInfoResponse()
		: base(Opcode.SMSG_QUERY_QUEST_INFO_RESPONSE, ConnectionType.Instance)
	{
	}

	public override void Write()
	{
		_worldPacket.WriteUInt32(QuestID);
		_worldPacket.WriteBit(Allow);
		_worldPacket.FlushBits();
		if (!Allow)
		{
			return;
		}
		_worldPacket.WriteUInt32(Info.QuestID);
		_worldPacket.WriteInt32(Info.QuestType);
		_worldPacket.WriteInt32(Info.QuestLevel);
		_worldPacket.WriteInt32(Info.QuestScalingFactionGroup);
		_worldPacket.WriteInt32(Info.QuestMaxScalingLevel);
		_worldPacket.WriteUInt32(Info.QuestPackageID);
		_worldPacket.WriteInt32(Info.MinLevel);
		_worldPacket.WriteInt32(Info.QuestSortID);
		_worldPacket.WriteUInt32(Info.QuestInfoID);
		_worldPacket.WriteUInt32(Info.SuggestedGroupNum);
		_worldPacket.WriteUInt32(Info.RewardNextQuest);
		_worldPacket.WriteUInt32(Info.RewardXPDifficulty);
		_worldPacket.WriteFloat(Info.RewardXPMultiplier);
		_worldPacket.WriteInt32(Info.RewardMoney);
		_worldPacket.WriteUInt32(Info.RewardMoneyDifficulty);
		_worldPacket.WriteFloat(Info.RewardMoneyMultiplier);
		_worldPacket.WriteUInt32(Info.RewardBonusMoney);
		for (uint num = 0u; num < 3; num++)
		{
			_worldPacket.WriteUInt32(Info.RewardDisplaySpell[num]);
		}
		_worldPacket.WriteUInt32(Info.RewardSpell);
		_worldPacket.WriteUInt32(Info.RewardHonor);
		_worldPacket.WriteFloat(Info.RewardKillHonor);
		_worldPacket.WriteInt32(Info.RewardArtifactXPDifficulty);
		_worldPacket.WriteFloat(Info.RewardArtifactXPMultiplier);
		_worldPacket.WriteInt32(Info.RewardArtifactCategoryID);
		_worldPacket.WriteUInt32(Info.StartItem);
		_worldPacket.WriteUInt32(Info.Flags);
		_worldPacket.WriteUInt32(Info.FlagsEx);
		_worldPacket.WriteUInt32(Info.FlagsEx2);
		for (uint num2 = 0u; num2 < 4; num2++)
		{
			_worldPacket.WriteUInt32(Info.RewardItems[num2]);
			_worldPacket.WriteUInt32(Info.RewardAmount[num2]);
			_worldPacket.WriteInt32(Info.ItemDrop[num2]);
			_worldPacket.WriteInt32(Info.ItemDropQuantity[num2]);
		}
		for (uint num3 = 0u; num3 < 6; num3++)
		{
			_worldPacket.WriteUInt32(Info.UnfilteredChoiceItems[num3].ItemID);
			_worldPacket.WriteUInt32(Info.UnfilteredChoiceItems[num3].Quantity);
			_worldPacket.WriteUInt32(Info.UnfilteredChoiceItems[num3].DisplayID);
		}
		_worldPacket.WriteUInt32(Info.POIContinent);
		_worldPacket.WriteFloat(Info.POIx);
		_worldPacket.WriteFloat(Info.POIy);
		_worldPacket.WriteUInt32(Info.POIPriority);
		_worldPacket.WriteUInt32(Info.RewardTitle);
		_worldPacket.WriteInt32(Info.RewardArenaPoints);
		_worldPacket.WriteUInt32(Info.RewardSkillLineID);
		_worldPacket.WriteUInt32(Info.RewardNumSkillUps);
		_worldPacket.WriteUInt32(Info.PortraitGiver);
		_worldPacket.WriteUInt32(Info.PortraitGiverMount);
		_worldPacket.WriteUInt32(Info.PortraitTurnIn);
		_worldPacket.WriteInt32(0);
		for (uint num4 = 0u; num4 < 5; num4++)
		{
			_worldPacket.WriteUInt32(Info.RewardFactionID[num4]);
			_worldPacket.WriteInt32(Info.RewardFactionValue[num4]);
			_worldPacket.WriteInt32(Info.RewardFactionOverride[num4]);
			_worldPacket.WriteInt32(Info.RewardFactionCapIn[num4]);
		}
		_worldPacket.WriteUInt32(Info.RewardFactionFlags);
		for (uint num5 = 0u; num5 < 4; num5++)
		{
			_worldPacket.WriteUInt32(Info.RewardCurrencyID[num5]);
			_worldPacket.WriteUInt32(Info.RewardCurrencyQty[num5]);
		}
		_worldPacket.WriteUInt32(Info.AcceptedSoundKitID);
		_worldPacket.WriteUInt32(Info.CompleteSoundKitID);
		_worldPacket.WriteUInt32(Info.AreaGroupID);
		_worldPacket.WriteUInt32(Info.TimeAllowed);
		_worldPacket.WriteInt32(Info.Objectives.Count);
		_worldPacket.WriteInt64(Info.AllowableRaces);
		_worldPacket.WriteInt32(Info.TreasurePickerID);
		_worldPacket.WriteInt32(Info.Expansion);
		_worldPacket.WriteBits(Info.LogTitle.GetByteCount(), 9);
		_worldPacket.WriteBits(Info.LogDescription.GetByteCount(), 12);
		_worldPacket.WriteBits(Info.QuestDescription.GetByteCount(), 12);
		_worldPacket.WriteBits(Info.AreaDescription.GetByteCount(), 9);
		_worldPacket.WriteBits(Info.PortraitGiverText.GetByteCount(), 10);
		_worldPacket.WriteBits(Info.PortraitGiverName.GetByteCount(), 8);
		_worldPacket.WriteBits(Info.PortraitTurnInText.GetByteCount(), 10);
		_worldPacket.WriteBits(Info.PortraitTurnInName.GetByteCount(), 8);
		_worldPacket.WriteBits(Info.QuestCompletionLog.GetByteCount(), 11);
		_worldPacket.WriteBit(Info.ReadyForTranslation);
		_worldPacket.FlushBits();
		foreach (QuestObjective objective in Info.Objectives)
		{
			_worldPacket.WriteUInt32(objective.Id);
			_worldPacket.WriteUInt8((byte)objective.Type);
			_worldPacket.WriteInt8(objective.StorageIndex);
			_worldPacket.WriteInt32(objective.ObjectID);
			_worldPacket.WriteInt32(objective.Amount);
			_worldPacket.WriteUInt32((uint)objective.Flags);
			_worldPacket.WriteUInt32(objective.Flags2);
			_worldPacket.WriteFloat(objective.ProgressBarWeight);
			_worldPacket.WriteInt32(objective.VisualEffects.Length);
			int[] visualEffects = objective.VisualEffects;
			foreach (int data in visualEffects)
			{
				_worldPacket.WriteInt32(data);
			}
			_worldPacket.WriteBits(objective.Description.GetByteCount(), 8);
			_worldPacket.FlushBits();
			_worldPacket.WriteString(objective.Description);
		}
		_worldPacket.WriteString(Info.LogTitle);
		_worldPacket.WriteString(Info.LogDescription);
		_worldPacket.WriteString(Info.QuestDescription);
		_worldPacket.WriteString(Info.AreaDescription);
		_worldPacket.WriteString(Info.PortraitGiverText);
		_worldPacket.WriteString(Info.PortraitGiverName);
		_worldPacket.WriteString(Info.PortraitTurnInText);
		_worldPacket.WriteString(Info.PortraitTurnInName);
		_worldPacket.WriteString(Info.QuestCompletionLog);
	}
}
