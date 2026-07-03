namespace HermesProxy.World.Server.Packets;

internal class RequestPartyMemberStats : ClientPacket
{
	public byte PartyIndex;

	public WowGuid128 TargetGUID;

	public RequestPartyMemberStats(WorldPacket packet)
		: base(packet)
	{
	}

	public override void Read()
	{
		PartyIndex = _worldPacket.ReadUInt8();
		TargetGUID = _worldPacket.ReadPackedGuid128();
	}
}
