namespace HermesProxy.World.Server.Packets;

internal class ChangeSubGroup : ClientPacket
{
	public WowGuid128 TargetGUID;

	public sbyte PartyIndex;

	public byte NewSubGroup;

	public ChangeSubGroup(WorldPacket packet)
		: base(packet)
	{
	}

	public override void Read()
	{
		TargetGUID = _worldPacket.ReadPackedGuid128();
		PartyIndex = _worldPacket.ReadInt8();
		NewSubGroup = _worldPacket.ReadUInt8();
	}
}
