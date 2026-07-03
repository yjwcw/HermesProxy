using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

public class ExplorationExperience : ServerPacket
{
	public uint AreaID;

	public uint Experience;

	public ExplorationExperience()
		: base(Opcode.SMSG_EXPLORATION_EXPERIENCE)
	{
	}

	public override void Write()
	{
		_worldPacket.WriteUInt32(AreaID);
		_worldPacket.WriteUInt32(Experience);
	}
}
