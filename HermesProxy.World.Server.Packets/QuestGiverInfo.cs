using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

public class QuestGiverInfo
{
	public WowGuid128 Guid;

	public QuestGiverStatusModern Status;

	public QuestGiverInfo()
	{
	}

	public QuestGiverInfo(WowGuid128 guid, QuestGiverStatusModern status)
	{
		Guid = guid;
		Status = status;
	}
}
