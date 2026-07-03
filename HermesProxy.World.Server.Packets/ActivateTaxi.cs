namespace HermesProxy.World.Server.Packets;

internal class ActivateTaxi : ClientPacket
{
	public WowGuid128 FlightMaster;

	public uint Node;

	public uint GroundMountID;

	public uint FlyingMountID;

	public ActivateTaxi(WorldPacket packet)
		: base(packet)
	{
	}

	public override void Read()
	{
		FlightMaster = _worldPacket.ReadPackedGuid128();
		Node = _worldPacket.ReadUInt32();
		GroundMountID = _worldPacket.ReadUInt32();
		FlyingMountID = _worldPacket.ReadUInt32();
	}
}
