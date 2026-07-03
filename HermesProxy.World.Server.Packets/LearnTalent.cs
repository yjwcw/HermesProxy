namespace HermesProxy.World.Server.Packets;

internal class LearnTalent : ClientPacket
{
	public uint TalentID;

	public ushort Rank;

	public LearnTalent(WorldPacket packet)
		: base(packet)
	{
	}

	public override void Read()
	{
		TalentID = _worldPacket.ReadUInt32();
		Rank = _worldPacket.ReadUInt16();
	}
}
