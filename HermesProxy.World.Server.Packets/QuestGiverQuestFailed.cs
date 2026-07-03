using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

internal class QuestGiverQuestFailed : ServerPacket
{
	public uint QuestID;

	public InventoryResult Reason;

	public QuestGiverQuestFailed()
		: base(Opcode.SMSG_QUEST_GIVER_QUEST_FAILED)
	{
	}

	public override void Write()
	{
		_worldPacket.WriteUInt32(QuestID);
		_worldPacket.WriteUInt32((uint)Reason);
	}
}
