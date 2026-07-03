using Framework.Constants;
using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

public class MoveSetSpeed : ServerPacket
{
	public WowGuid128 MoverGUID;

	public uint MoveCounter;

	public float Speed = 1f;

	public MoveSetSpeed(Opcode opcode)
		: base(opcode, ConnectionType.Instance)
	{
	}

	public override void Write()
	{
		_worldPacket.WritePackedGuid128(MoverGUID);
		_worldPacket.WriteUInt32(MoveCounter);
		_worldPacket.WriteFloat(Speed);
	}
}
