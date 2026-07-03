namespace HermesProxy.World.Server.Packets;

public class GuildBankTextQuery : ClientPacket
{
	public int Tab;

	public GuildBankTextQuery(WorldPacket packet)
		: base(packet)
	{
	}

	public override void Read()
	{
		Tab = _worldPacket.ReadInt32();
	}
}
