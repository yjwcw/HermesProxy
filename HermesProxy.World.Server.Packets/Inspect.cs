namespace HermesProxy.World.Server.Packets;

public class Inspect : ClientPacket
{
	public WowGuid128 Target;

	public Inspect(WorldPacket packet)
		: base(packet)
	{
	}

	public override void Read()
	{
		Target = _worldPacket.ReadPackedGuid128();
	}
}
