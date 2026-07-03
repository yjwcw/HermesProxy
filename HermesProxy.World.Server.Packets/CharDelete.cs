namespace HermesProxy.World.Server.Packets;

public class CharDelete : ClientPacket
{
	public WowGuid128 Guid;

	public CharDelete(WorldPacket packet)
		: base(packet)
	{
	}

	public override void Read()
	{
		Guid = _worldPacket.ReadPackedGuid128();
	}
}
