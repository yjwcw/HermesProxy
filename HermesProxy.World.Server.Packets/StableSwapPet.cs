namespace HermesProxy.World.Server.Packets;

internal class StableSwapPet : ClientPacket
{
	public uint PetNumber;

	public WowGuid128 StableMaster;

	public StableSwapPet(WorldPacket packet)
		: base(packet)
	{
	}

	public override void Read()
	{
		PetNumber = _worldPacket.ReadUInt32();
		StableMaster = _worldPacket.ReadPackedGuid128();
	}
}
