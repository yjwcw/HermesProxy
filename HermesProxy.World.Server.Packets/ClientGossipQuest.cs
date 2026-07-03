using System;

namespace HermesProxy.World.Server.Packets;

public class ClientGossipQuest
{
	public uint QuestID;

	public uint ContentTuningID;

	public int QuestType;

	public int QuestLevel;

	public int QuestMaxLevel = 255;

	public bool Repeatable;

	public string QuestTitle;

	public uint QuestFlags = 8u;

	public uint QuestFlagsEx;

	public void Write(WorldPacket data)
	{
		data.WriteUInt32(QuestID);
		data.WriteUInt32(ContentTuningID);
		data.WriteInt32(QuestType);
		data.WriteInt32(QuestLevel);
		data.WriteInt32(QuestMaxLevel);
		data.WriteUInt32(QuestFlags);
		data.WriteUInt32(QuestFlagsEx);
		data.WriteBit(Repeatable);
		data.WriteBits(QuestTitle.GetByteCount(), 9);
		data.FlushBits();
		data.WriteString(QuestTitle);
	}
}
