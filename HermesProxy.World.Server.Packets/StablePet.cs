namespace HermesProxy.World.Server.Packets;

internal class StablePet : ClientPacket
{
	public WowGuid128 StableMaster;

	public StablePet(WorldPacket packet)
		: base(packet)
	{
	}

	public override void Read()
	{
		StableMaster = _worldPacket.ReadPackedGuid128();
	}
}
