namespace HermesProxy.World.Server.Packets;

public class MovementAckMessage : ClientPacket
{
	public WowGuid128 MoverGUID;

	public MovementAck Ack;

	public MovementAckMessage(WorldPacket packet)
		: base(packet)
	{
	}

	public override void Read()
	{
		MoverGUID = _worldPacket.ReadPackedGuid128();
		Ack.Read(_worldPacket);
	}
}
