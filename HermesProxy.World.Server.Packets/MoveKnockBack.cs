using Framework.Constants;
using Framework.GameMath;
using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

internal class MoveKnockBack : ServerPacket
{
	public WowGuid128 MoverGUID;

	public uint MoveCounter;

	public Vector2 Direction;

	public float HorizontalSpeed;

	public float VerticalSpeed;

	public MoveKnockBack()
		: base(Opcode.SMSG_MOVE_KNOCK_BACK, ConnectionType.Instance)
	{
	}

	public override void Write()
	{
		_worldPacket.WritePackedGuid128(MoverGUID);
		_worldPacket.WriteUInt32(MoveCounter);
		_worldPacket.WriteVector2(Direction);
		_worldPacket.WriteFloat(HorizontalSpeed);
		_worldPacket.WriteFloat(VerticalSpeed);
	}
}
