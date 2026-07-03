using Framework.Constants;
using Framework.GameMath;
using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

public class MoveTeleport : ServerPacket
{
	public Vector3 Position;

	public VehicleTeleport Vehicle;

	public uint MoveCounter;

	public WowGuid128 MoverGUID;

	public WowGuid128 TransportGUID;

	public float Orientation;

	public byte PreloadWorld;

	public MoveTeleport()
		: base(Opcode.SMSG_MOVE_TELEPORT, ConnectionType.Instance)
	{
	}

	public override void Write()
	{
		_worldPacket.WritePackedGuid128(MoverGUID);
		_worldPacket.WriteUInt32(MoveCounter);
		_worldPacket.WriteVector3(Position);
		_worldPacket.WriteFloat(Orientation);
		_worldPacket.WriteUInt8(PreloadWorld);
		_worldPacket.WriteBit(TransportGUID != null);
		_worldPacket.WriteBit(Vehicle != null);
		_worldPacket.FlushBits();
		if (Vehicle != null)
		{
			_worldPacket.WriteInt8(Vehicle.VehicleSeatIndex);
			_worldPacket.WriteBit(Vehicle.VehicleExitVoluntary);
			_worldPacket.WriteBit(Vehicle.VehicleExitTeleport);
			_worldPacket.FlushBits();
		}
		if (TransportGUID != null)
		{
			_worldPacket.WritePackedGuid128(TransportGUID);
		}
	}
}
