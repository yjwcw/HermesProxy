namespace HermesProxy.World.Server.Packets;

internal class MoveTimeSkipped : ClientPacket
{
	public WowGuid128 MoverGUID;

	public uint TimeSkipped;

	public MoveTimeSkipped(WorldPacket packet)
		: base(packet)
	{
	}

	public override void Read()
	{
		MoverGUID = _worldPacket.ReadPackedGuid128();
		TimeSkipped = _worldPacket.ReadUInt32();
	}
}
