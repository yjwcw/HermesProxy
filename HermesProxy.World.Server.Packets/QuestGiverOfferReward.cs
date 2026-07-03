using System.Collections.Generic;

namespace HermesProxy.World.Server.Packets;

public class QuestGiverOfferReward
{
	public WowGuid128 QuestGiverGUID;

	public uint QuestGiverCreatureID;

	public uint QuestID;

	public bool AutoLaunched;

	public uint SuggestedPartyMembers;

	public QuestRewards Rewards = new QuestRewards();

	public List<QuestDescEmote> Emotes = new List<QuestDescEmote>();

	public uint[] QuestFlags = new uint[2];

	public void Write(WorldPacket data)
	{
		data.WritePackedGuid128(QuestGiverGUID);
		data.WriteUInt32(QuestGiverCreatureID);
		data.WriteUInt32(QuestID);
		data.WriteUInt32(QuestFlags[0]);
		data.WriteUInt32(QuestFlags[1]);
		data.WriteUInt32(SuggestedPartyMembers);
		data.WriteInt32(Emotes.Count);
		foreach (QuestDescEmote emote in Emotes)
		{
			data.WriteUInt32(emote.Type);
			data.WriteUInt32(emote.Delay);
		}
		data.WriteBit(AutoLaunched);
		data.WriteBit(bit: false);
		data.FlushBits();
		Rewards.Write(data);
	}
}
