using Framework.Constants;
using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

public class AIReaction : ServerPacket
{
	public WowGuid128 UnitGUID;

	public uint Reaction;

	public AIReaction()
		: base(Opcode.SMSG_AI_REACTION, ConnectionType.Instance)
	{
	}

	public override void Write()
	{
		_worldPacket.WritePackedGuid128(UnitGUID);
		_worldPacket.WriteUInt32(Reaction);
	}
}
