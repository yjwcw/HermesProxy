namespace HermesProxy.World.Server.Packets;

internal class PartyUninvite : ClientPacket
{
	public byte PartyIndex;

	public WowGuid128 TargetGUID;

	public string Reason;

	public PartyUninvite(WorldPacket packet)
		: base(packet)
	{
	}

	public override void Read()
	{
		PartyIndex = _worldPacket.ReadUInt8();
		TargetGUID = _worldPacket.ReadPackedGuid128();
		byte length = _worldPacket.ReadBits<byte>(8);
		Reason = _worldPacket.ReadString(length);
	}
}
