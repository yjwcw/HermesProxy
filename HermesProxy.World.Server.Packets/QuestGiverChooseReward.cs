namespace HermesProxy.World.Server.Packets;

public class QuestGiverChooseReward : ClientPacket
{
	public WowGuid128 QuestGiverGUID;

	public uint QuestID;

	public QuestChoiceItem Choice = new QuestChoiceItem();

	public QuestGiverChooseReward(WorldPacket packet)
		: base(packet)
	{
	}

	public override void Read()
	{
		QuestGiverGUID = _worldPacket.ReadPackedGuid128();
		QuestID = _worldPacket.ReadUInt32();
		Choice.Read(_worldPacket);
	}
}
