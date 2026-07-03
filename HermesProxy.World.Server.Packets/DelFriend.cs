namespace HermesProxy.World.Server.Packets;

public class DelFriend : ClientPacket
{
	public uint VirtualRealmAddress;

	public WowGuid128 Guid;

	public DelFriend(WorldPacket packet)
		: base(packet)
	{
	}

	public override void Read()
	{
		VirtualRealmAddress = _worldPacket.ReadUInt32();
		Guid = _worldPacket.ReadPackedGuid128();
	}
}
