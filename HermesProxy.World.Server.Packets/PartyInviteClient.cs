namespace HermesProxy.World.Server.Packets;

internal class PartyInviteClient : ClientPacket
{
	public byte PartyIndex;

	public uint VirtualRealmAddress;

	public WowGuid128 TargetGUID;

	public string TargetName;

	public string TargetRealm;

	public PartyInviteClient(WorldPacket packet)
		: base(packet)
	{
	}

	public override void Read()
	{
		PartyIndex = _worldPacket.ReadUInt8();
		uint length = _worldPacket.ReadBits<uint>(9);
		uint length2 = _worldPacket.ReadBits<uint>(9);
		VirtualRealmAddress = _worldPacket.ReadUInt32();
		TargetGUID = _worldPacket.ReadPackedGuid128();
		TargetName = _worldPacket.ReadString(length);
		TargetRealm = _worldPacket.ReadString(length2);
	}
}
