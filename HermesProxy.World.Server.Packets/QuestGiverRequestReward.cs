namespace HermesProxy.World.Server.Packets;

public class QuestGiverRequestReward : ClientPacket
{
	public WowGuid128 QuestGiverGUID;

	public uint QuestID;

	public QuestGiverRequestReward(WorldPacket packet)
		: base(packet)
	{
	}

	public override void Read()
	{
		QuestGiverGUID = _worldPacket.ReadPackedGuid128();
		QuestID = _worldPacket.ReadUInt32();
	}
}
