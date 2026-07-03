using Framework.GameMath;
using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

public class NewWorld : ServerPacket
{
	public uint MapID;

	public uint Reason;

	public Vector3 Position;

	public float Orientation;

	public Vector3 MovementOffset;

	public NewWorld()
		: base(Opcode.SMSG_NEW_WORLD)
	{
	}

	public override void Write()
	{
		_worldPacket.WriteUInt32(MapID);
		_worldPacket.WriteVector3(Position);
		_worldPacket.WriteFloat(Orientation);
		_worldPacket.WriteUInt32(Reason);
		_worldPacket.WriteVector3(MovementOffset);
	}
}
