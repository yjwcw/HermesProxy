using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

internal class QuestUpdateStatus : ServerPacket
{
	public uint QuestID;

	public QuestUpdateStatus(Opcode opcode)
		: base(opcode)
	{
	}

	public override void Write()
	{
		_worldPacket.WriteUInt32(QuestID);
	}
}
