namespace HermesProxy.World.Server.Packets;

internal class UnstablePet : ClientPacket
{
	public uint PetNumber;

	public WowGuid128 StableMaster;

	public UnstablePet(WorldPacket packet)
		: base(packet)
	{
	}

	public override void Read()
	{
		PetNumber = _worldPacket.ReadUInt32();
		StableMaster = _worldPacket.ReadPackedGuid128();
	}
}
