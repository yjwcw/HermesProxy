namespace HermesProxy.World.Server.Packets;

internal class MoveTeleportAck : ClientPacket
{
	public WowGuid128 MoverGUID;

	public uint MoveCounter;

	public uint MoveTime;

	public MoveTeleportAck(WorldPacket packet)
		: base(packet)
	{
	}

	public override void Read()
	{
		MoverGUID = _worldPacket.ReadPackedGuid128();
		MoveCounter = _worldPacket.ReadUInt32();
		MoveTime = _worldPacket.ReadUInt32();
	}
}
