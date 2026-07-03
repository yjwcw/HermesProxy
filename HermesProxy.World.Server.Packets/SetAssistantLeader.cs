namespace HermesProxy.World.Server.Packets;

internal class SetAssistantLeader : ClientPacket
{
	public byte PartyIndex;

	public WowGuid128 TargetGUID;

	public bool Apply;

	public SetAssistantLeader(WorldPacket packet)
		: base(packet)
	{
	}

	public override void Read()
	{
		PartyIndex = _worldPacket.ReadUInt8();
		TargetGUID = _worldPacket.ReadPackedGuid128();
		Apply = _worldPacket.HasBit();
	}
}
