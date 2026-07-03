namespace HermesProxy.World.Server.Packets;

internal class PetInfoRequest : ClientPacket
{
	public PetInfoRequest(WorldPacket packet)
		: base(packet)
	{
	}

	public override void Read()
	{
	}
}
