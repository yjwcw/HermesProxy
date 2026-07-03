using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

public class StandStateUpdate : ServerPacket
{
	public uint AnimKitID;

	public byte StandState;

	public StandStateUpdate()
		: base(Opcode.SMSG_STAND_STATE_UPDATE)
	{
	}

	public override void Write()
	{
		_worldPacket.WriteUInt32(AnimKitID);
		_worldPacket.WriteUInt8(StandState);
	}
}
