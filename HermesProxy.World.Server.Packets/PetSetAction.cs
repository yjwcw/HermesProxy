namespace HermesProxy.World.Server.Packets;

internal class PetSetAction : ClientPacket
{
	public WowGuid128 PetGUID;

	public uint Index;

	public uint Action;

	public PetSetAction(WorldPacket packet)
		: base(packet)
	{
	}

	public override void Read()
	{
		PetGUID = _worldPacket.ReadPackedGuid128();
		Index = _worldPacket.ReadUInt32();
		Action = _worldPacket.ReadUInt32();
	}
}
