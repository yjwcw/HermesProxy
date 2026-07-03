using Framework.Constants;
using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

public class MoveSplineSetSpeed : ServerPacket
{
	public WowGuid128 MoverGUID;

	public float Speed = 1f;

	public MoveSplineSetSpeed(Opcode opcode)
		: base(opcode, ConnectionType.Instance)
	{
	}

	public override void Write()
	{
		_worldPacket.WritePackedGuid128(MoverGUID);
		_worldPacket.WriteFloat(Speed);
	}
}
