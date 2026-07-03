namespace HermesProxy.World.Server.Packets;

internal class SetPartyLeader : ClientPacket
{
	public sbyte PartyIndex;

	public WowGuid128 TargetGUID;

	public SetPartyLeader(WorldPacket packet)
		: base(packet)
	{
	}

	public override void Read()
	{
		PartyIndex = _worldPacket.ReadInt8();
		TargetGUID = _worldPacket.ReadPackedGuid128();
	}
}
