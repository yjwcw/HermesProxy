namespace HermesProxy.World.Server.Packets;

internal class RequestStabledPets : ClientPacket
{
	public WowGuid128 StableMaster;

	public RequestStabledPets(WorldPacket packet)
		: base(packet)
	{
	}

	public override void Read()
	{
		StableMaster = _worldPacket.ReadPackedGuid128();
	}
}
