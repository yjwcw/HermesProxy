namespace HermesProxy.World.Server.Packets;

internal class QuestConfirmAcceptResponse : ClientPacket
{
	public uint QuestID;

	public QuestConfirmAcceptResponse(WorldPacket packet)
		: base(packet)
	{
	}

	public override void Read()
	{
		QuestID = _worldPacket.ReadUInt32();
	}
}
