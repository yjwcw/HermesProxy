using HermesProxy.World.Enums;
using HermesProxy.World.Objects;

namespace HermesProxy.World.Server.Packets;

public class MoveUpdateKnockBack : ServerPacket
{
	public WowGuid128 MoverGUID;

	public MovementInfo MoveInfo;

	public MoveUpdateKnockBack()
		: base(Opcode.SMSG_MOVE_UPDATE_KNOCK_BACK)
	{
	}

	public override void Write()
	{
		MoveInfo.WriteMovementInfoModern(_worldPacket, MoverGUID);
	}
}
