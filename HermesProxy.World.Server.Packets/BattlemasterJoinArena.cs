namespace HermesProxy.World.Server.Packets;

internal class BattlemasterJoinArena : ClientPacket
{
	public WowGuid128 Guid;

	public byte TeamIndex;

	public byte Roles;

	public BattlemasterJoinArena(WorldPacket packet)
		: base(packet)
	{
	}

	public override void Read()
	{
		Guid = _worldPacket.ReadPackedGuid128();
		TeamIndex = _worldPacket.ReadUInt8();
		Roles = _worldPacket.ReadUInt8();
	}
}
