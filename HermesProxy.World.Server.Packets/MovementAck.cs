using HermesProxy.World.Objects;

namespace HermesProxy.World.Server.Packets;

public struct MovementAck
{
	public MovementInfo MoveInfo;

	public uint MoveCounter;

	public void Read(WorldPacket data)
	{
		MoveInfo = new MovementInfo();
		MoveInfo.ReadMovementInfoModern(data);
		MoveCounter = data.ReadUInt32();
	}
}
