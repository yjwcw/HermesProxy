using Framework.Constants;
using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

internal class QuestUpdateAddCreditSimple : ServerPacket
{
	public uint QuestID;

	public int ObjectID;

	public QuestObjectiveType ObjectiveType;

	public QuestUpdateAddCreditSimple()
		: base(Opcode.SMSG_QUEST_UPDATE_ADD_CREDIT_SIMPLE, ConnectionType.Instance)
	{
	}

	public override void Write()
	{
		_worldPacket.WriteUInt32(QuestID);
		_worldPacket.WriteInt32(ObjectID);
		_worldPacket.WriteUInt8((byte)ObjectiveType);
	}
}
