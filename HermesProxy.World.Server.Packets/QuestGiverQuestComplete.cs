using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

public class QuestGiverQuestComplete : ServerPacket
{
	public uint QuestID;

	public uint XPReward;

	public long MoneyReward;

	public uint SkillLineIDReward;

	public uint NumSkillUpsReward;

	public bool UseQuestReward;

	public bool LaunchGossip;

	public bool LaunchQuest = true;

	public bool HideChatMessage;

	public ItemInstance ItemReward = new ItemInstance();

	public QuestGiverQuestComplete()
		: base(Opcode.SMSG_QUEST_GIVER_QUEST_COMPLETE)
	{
	}

	public override void Write()
	{
		_worldPacket.WriteUInt32(QuestID);
		_worldPacket.WriteUInt32(XPReward);
		_worldPacket.WriteInt64(MoneyReward);
		_worldPacket.WriteUInt32(SkillLineIDReward);
		_worldPacket.WriteUInt32(NumSkillUpsReward);
		_worldPacket.WriteBit(UseQuestReward);
		_worldPacket.WriteBit(LaunchGossip);
		_worldPacket.WriteBit(LaunchQuest);
		_worldPacket.WriteBit(HideChatMessage);
		ItemReward.Write(_worldPacket);
	}
}
