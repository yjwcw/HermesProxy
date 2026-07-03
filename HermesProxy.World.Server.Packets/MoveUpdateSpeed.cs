using Framework.Constants;
using HermesProxy.World.Enums;
using HermesProxy.World.Objects;

namespace HermesProxy.World.Server.Packets;

public class MoveUpdateSpeed : ServerPacket
{
	public WowGuid128 MoverGUID;

	public MovementInfo MoveInfo;

	public float Speed = 1f;

	public MoveUpdateSpeed(Opcode opcode)
		: base(opcode, ConnectionType.Instance)
	{
	}

	public override void Write()
	{
		MoveInfo.WriteMovementInfoModern(_worldPacket, MoverGUID);
		_worldPacket.WriteFloat(Speed);
	}
}
