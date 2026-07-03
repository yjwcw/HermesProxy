namespace HermesProxy.World.Server.Packets;

internal class UnlearnSkill : ClientPacket
{
	public uint SkillLine;

	public UnlearnSkill(WorldPacket packet)
		: base(packet)
	{
	}

	public override void Read()
	{
		SkillLine = _worldPacket.ReadUInt32();
	}
}
