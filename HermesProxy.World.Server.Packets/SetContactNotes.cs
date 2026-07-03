namespace HermesProxy.World.Server.Packets;

public class SetContactNotes : ClientPacket
{
	public uint VirtualRealmAddress;

	public WowGuid128 Guid;

	public string Notes;

	public SetContactNotes(WorldPacket packet)
		: base(packet)
	{
	}

	public override void Read()
	{
		VirtualRealmAddress = _worldPacket.ReadUInt32();
		Guid = _worldPacket.ReadPackedGuid128();
		Notes = _worldPacket.ReadString(_worldPacket.ReadBits<uint>(10));
	}
}
