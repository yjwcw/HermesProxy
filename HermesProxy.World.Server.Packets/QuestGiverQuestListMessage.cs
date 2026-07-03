using System;
using System.Collections.Generic;
using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

public class QuestGiverQuestListMessage : ServerPacket
{
	public WowGuid128 QuestGiverGUID;

	public uint GreetEmoteDelay;

	public uint GreetEmoteType;

	public List<ClientGossipQuest> QuestOptions = new List<ClientGossipQuest>();

	public string Greeting = "";

	public QuestGiverQuestListMessage()
		: base(Opcode.SMSG_QUEST_GIVER_QUEST_LIST_MESSAGE)
	{
	}

	public override void Write()
	{
		_worldPacket.WritePackedGuid128(QuestGiverGUID);
		_worldPacket.WriteUInt32(GreetEmoteDelay);
		_worldPacket.WriteUInt32(GreetEmoteType);
		_worldPacket.WriteInt32(QuestOptions.Count);
		_worldPacket.WriteBits(Greeting.GetByteCount(), 11);
		_worldPacket.FlushBits();
		foreach (ClientGossipQuest questOption in QuestOptions)
		{
			questOption.Write(_worldPacket);
		}
		_worldPacket.WriteString(Greeting);
	}
}
