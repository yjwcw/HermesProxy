using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

internal class QuestPushResultResponse : ClientPacket
{
	public WowGuid128 SenderGUID;

	public uint QuestID;

	public QuestPushReason Result;

	public QuestPushResultResponse(WorldPacket packet)
		: base(packet)
	{
	}

	public override void Read()
	{
		SenderGUID = _worldPacket.ReadPackedGuid128();
		QuestID = _worldPacket.ReadUInt32();
		Result = (QuestPushReason)_worldPacket.ReadUInt8();
	}
}
