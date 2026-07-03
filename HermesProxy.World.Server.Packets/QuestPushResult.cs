using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

internal class QuestPushResult : ServerPacket
{
	public WowGuid128 SenderGUID;

	public QuestPushReason Result;

	public QuestPushResult()
		: base(Opcode.SMSG_QUEST_PUSH_RESULT)
	{
	}

	public override void Write()
	{
		_worldPacket.WritePackedGuid128(SenderGUID);
		_worldPacket.WriteUInt8((byte)Result);
	}
}
