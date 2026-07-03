using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

internal class MoveSetCollisionHeight : ServerPacket
{
	public enum UpdateCollisionHeightReason : byte
	{
		Scale,
		Mount,
		Force
	}

	public WowGuid128 MoverGUID;

	public uint SequenceIndex = 1u;

	public float Height = 1f;

	public float Scale = 1f;

	public UpdateCollisionHeightReason Reason;

	public uint MountDisplayID;

	public int ScaleDuration = 2000;

	public MoveSetCollisionHeight()
		: base(Opcode.SMSG_MOVE_SET_COLLISION_HEIGHT)
	{
	}

	public override void Write()
	{
		_worldPacket.WritePackedGuid128(MoverGUID);
		_worldPacket.WriteUInt32(SequenceIndex);
		_worldPacket.WriteFloat(Height);
		_worldPacket.WriteFloat(Scale);
		_worldPacket.WriteByteEnum(Reason);
		_worldPacket.WriteUInt32(MountDisplayID);
		_worldPacket.WriteInt32(ScaleDuration);
	}
}
