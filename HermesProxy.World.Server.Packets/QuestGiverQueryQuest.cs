namespace HermesProxy.World.Server.Packets;

public class QuestGiverQueryQuest : ClientPacket
{
	public WowGuid128 QuestGiverGUID;

	public uint QuestID;

	public bool RespondToGiver;

	public QuestGiverQueryQuest(WorldPacket packet)
		: base(packet)
	{
	}

	public override void Read()
	{
		QuestGiverGUID = _worldPacket.ReadPackedGuid128();
		QuestID = _worldPacket.ReadUInt32();
		RespondToGiver = _worldPacket.HasBit();
	}
}
