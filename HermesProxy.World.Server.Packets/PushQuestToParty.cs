namespace HermesProxy.World.Server.Packets;

internal class PushQuestToParty : ClientPacket
{
	public uint QuestID;

	public PushQuestToParty(WorldPacket packet)
		: base(packet)
	{
	}

	public override void Read()
	{
		QuestID = _worldPacket.ReadUInt32();
	}
}
