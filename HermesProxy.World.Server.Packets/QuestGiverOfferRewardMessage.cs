using System;
using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

public class QuestGiverOfferRewardMessage : ServerPacket
{
	public uint PortraitTurnIn;

	public uint PortraitGiver;

	public uint PortraitGiverMount;

	public uint PortraitGiverModelSceneID;

	public string QuestTitle = "";

	public string RewardText = "";

	public string PortraitGiverText = "";

	public string PortraitGiverName = "";

	public string PortraitTurnInText = "";

	public string PortraitTurnInName = "";

	public QuestGiverOfferReward QuestData = new QuestGiverOfferReward();

	public uint QuestPackageID;

	public QuestGiverOfferRewardMessage()
		: base(Opcode.SMSG_QUEST_GIVER_OFFER_REWARD_MESSAGE)
	{
	}

	public override void Write()
	{
		QuestData.Write(_worldPacket);
		_worldPacket.WriteUInt32(QuestPackageID);
		_worldPacket.WriteUInt32(PortraitGiver);
		_worldPacket.WriteUInt32(PortraitGiverMount);
		_worldPacket.WriteUInt32(PortraitGiverModelSceneID);
		_worldPacket.WriteUInt32(PortraitTurnIn);
		_worldPacket.WriteBits(QuestTitle.GetByteCount(), 9);
		_worldPacket.WriteBits(RewardText.GetByteCount(), 12);
		_worldPacket.WriteBits(PortraitGiverText.GetByteCount(), 10);
		_worldPacket.WriteBits(PortraitGiverName.GetByteCount(), 8);
		_worldPacket.WriteBits(PortraitTurnInText.GetByteCount(), 10);
		_worldPacket.WriteBits(PortraitTurnInName.GetByteCount(), 8);
		_worldPacket.WriteString(QuestTitle);
		_worldPacket.WriteString(RewardText);
		_worldPacket.WriteString(PortraitGiverText);
		_worldPacket.WriteString(PortraitGiverName);
		_worldPacket.WriteString(PortraitTurnInText);
		_worldPacket.WriteString(PortraitTurnInName);
	}
}
