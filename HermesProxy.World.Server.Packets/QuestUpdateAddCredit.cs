using Framework.Constants;
using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

public class QuestUpdateAddCredit : ServerPacket
{
	public WowGuid128 VictimGUID;

	public int ObjectID;

	public uint QuestID;

	public ushort Count;

	public ushort Required;

	public QuestObjectiveType ObjectiveType;

	public QuestUpdateAddCredit()
		: base(Opcode.SMSG_QUEST_UPDATE_ADD_CREDIT, ConnectionType.Instance)
	{
	}

	public override void Write()
	{
		_worldPacket.WritePackedGuid128(VictimGUID);
		_worldPacket.WriteUInt32(QuestID);
		_worldPacket.WriteInt32(ObjectID);
		_worldPacket.WriteUInt16(Count);
		_worldPacket.WriteUInt16(Required);
		_worldPacket.WriteUInt8((byte)ObjectiveType);
	}
}
