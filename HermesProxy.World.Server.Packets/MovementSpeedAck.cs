namespace HermesProxy.World.Server.Packets;

public class MovementSpeedAck : ClientPacket
{
	public WowGuid128 MoverGUID;

	public MovementAck Ack;

	public float Speed;

	public MovementSpeedAck(WorldPacket packet)
		: base(packet)
	{
	}

	public override void Read()
	{
		MoverGUID = _worldPacket.ReadPackedGuid128();
		Ack.Read(_worldPacket);
		Speed = _worldPacket.ReadFloat();
	}
}
