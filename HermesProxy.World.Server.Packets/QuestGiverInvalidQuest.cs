using System;
using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

internal class QuestGiverInvalidQuest : ServerPacket
{
	public QuestFailedReasons Reason;

	public int ContributionRewardID;

	public bool SendErrorMessage = true;

	public string ReasonText = "";

	public QuestGiverInvalidQuest()
		: base(Opcode.SMSG_QUEST_GIVER_INVALID_QUEST)
	{
	}

	public override void Write()
	{
		_worldPacket.WriteUInt32((uint)Reason);
		_worldPacket.WriteInt32(ContributionRewardID);
		_worldPacket.WriteBit(SendErrorMessage);
		_worldPacket.WriteBits(ReasonText.GetByteCount(), 9);
		_worldPacket.FlushBits();
		_worldPacket.WriteString(ReasonText);
	}
}
