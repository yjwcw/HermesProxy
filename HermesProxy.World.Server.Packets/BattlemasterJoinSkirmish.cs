namespace HermesProxy.World.Server.Packets;

internal class BattlemasterJoinSkirmish : ClientPacket
{
	public WowGuid128 Guid;

	public byte Roles;

	public byte TeamSize;

	public bool AsGroup;

	public bool Requeue;

	public BattlemasterJoinSkirmish(WorldPacket packet)
		: base(packet)
	{
	}

	public override void Read()
	{
		Guid = _worldPacket.ReadPackedGuid128();
		Roles = _worldPacket.ReadUInt8();
		TeamSize = _worldPacket.ReadUInt8();
		AsGroup = _worldPacket.HasBit();
		Requeue = _worldPacket.HasBit();
	}
}
