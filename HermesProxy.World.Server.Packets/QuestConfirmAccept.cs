using System;
using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

internal class QuestConfirmAccept : ServerPacket
{
	public WowGuid128 InitiatedBy;

	public uint QuestID;

	public string QuestTitle;

	public QuestConfirmAccept()
		: base(Opcode.SMSG_QUEST_CONFIRM_ACCEPT)
	{
	}

	public override void Write()
	{
		_worldPacket.WriteUInt32(QuestID);
		_worldPacket.WritePackedGuid128(InitiatedBy);
		_worldPacket.WriteBits(QuestTitle.GetByteCount(), 10);
		_worldPacket.WriteString(QuestTitle);
	}
}
