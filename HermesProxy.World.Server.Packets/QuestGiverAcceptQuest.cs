namespace HermesProxy.World.Server.Packets;

public class QuestGiverAcceptQuest : ClientPacket
{
	public WowGuid128 QuestGiverGUID;

	public uint QuestID;

	public bool StartCheat;

	public QuestGiverAcceptQuest(WorldPacket packet)
		: base(packet)
	{
	}

	public override void Read()
	{
		QuestGiverGUID = _worldPacket.ReadPackedGuid128();
		QuestID = _worldPacket.ReadUInt32();
		StartCheat = _worldPacket.HasBit();
	}
}
