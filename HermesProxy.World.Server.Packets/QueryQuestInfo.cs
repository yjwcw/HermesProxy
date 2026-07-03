namespace HermesProxy.World.Server.Packets;

public class QueryQuestInfo : ClientPacket
{
	public WowGuid128 QuestGiver;

	public uint QuestID;

	public QueryQuestInfo(WorldPacket packet)
		: base(packet)
	{
	}

	public override void Read()
	{
		QuestID = _worldPacket.ReadUInt32();
		QuestGiver = _worldPacket.ReadPackedGuid128();
	}
}
