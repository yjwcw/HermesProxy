using Framework.Constants;
using HermesProxy.World.Enums;
using HermesProxy.World.Objects;

namespace HermesProxy.World.Server.Packets;

public class MoveUpdate : ServerPacket
{
	public WowGuid128 MoverGUID;

	public MovementInfo MoveInfo;

	public MoveUpdate()
		: base(Opcode.SMSG_MOVE_UPDATE, ConnectionType.Instance)
	{
	}

	public override void Write()
	{
		MoveInfo.WriteMovementInfoModern(_worldPacket, MoverGUID);
	}
}
