using System.Collections.Generic;
using Framework.Constants;
using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

public class QuestGiverStatusMultiple : ServerPacket
{
	public List<QuestGiverInfo> QuestGivers = new List<QuestGiverInfo>();

	public QuestGiverStatusMultiple()
		: base(Opcode.SMSG_QUEST_GIVER_STATUS_MULTIPLE, ConnectionType.Instance)
	{
	}

	public override void Write()
	{
		_worldPacket.WriteInt32(QuestGivers.Count);
		foreach (QuestGiverInfo questGiver in QuestGivers)
		{
			_worldPacket.WritePackedGuid128(questGiver.Guid);
			_worldPacket.WriteUInt32((uint)questGiver.Status);
		}
	}
}
