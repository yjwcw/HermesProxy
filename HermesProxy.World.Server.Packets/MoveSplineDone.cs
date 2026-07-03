using HermesProxy.World.Objects;

namespace HermesProxy.World.Server.Packets;

internal class MoveSplineDone : ClientPacket
{
	public WowGuid128 Guid;

	public MovementInfo MoveInfo;

	public int SplineID;

	public MoveSplineDone(WorldPacket packet)
		: base(packet)
	{
	}

	public override void Read()
	{
		Guid = _worldPacket.ReadPackedGuid128();
		MoveInfo = new MovementInfo();
		MoveInfo.ReadMovementInfoModern(_worldPacket);
		SplineID = _worldPacket.ReadInt32();
	}
}
