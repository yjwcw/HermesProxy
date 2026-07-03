namespace HermesProxy.World.Server.Packets;

public class QuestGiverCompleteQuest : ClientPacket
{
	public WowGuid128 QuestGiverGUID;

	public uint QuestID;

	public bool FromScript;

	public QuestGiverCompleteQuest(WorldPacket packet)
		: base(packet)
	{
	}

	public override void Read()
	{
		QuestGiverGUID = _worldPacket.ReadPackedGuid128();
		QuestID = _worldPacket.ReadUInt32();
		FromScript = _worldPacket.HasBit();
	}
}
