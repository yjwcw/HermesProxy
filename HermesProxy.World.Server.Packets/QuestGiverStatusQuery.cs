namespace HermesProxy.World.Server.Packets;

public class QuestGiverStatusQuery : ClientPacket
{
	public WowGuid128 QuestGiverGUID;

	public QuestGiverStatusQuery(WorldPacket packet)
		: base(packet)
	{
	}

	public override void Read()
	{
		QuestGiverGUID = _worldPacket.ReadPackedGuid128();
	}
}
