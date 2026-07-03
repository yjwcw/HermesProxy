using HermesProxy.World.Objects;

namespace HermesProxy.World.Server.Packets;

public class ClientPlayerMovement : ClientPacket
{
	public WowGuid128 Guid;

	public MovementInfo MoveInfo;

	public ClientPlayerMovement(WorldPacket packet)
		: base(packet)
	{
	}

	public override void Read()
	{
		Guid = _worldPacket.ReadPackedGuid128();
		MoveInfo = new MovementInfo();
		MoveInfo.ReadMovementInfoModern(_worldPacket);
	}
}
