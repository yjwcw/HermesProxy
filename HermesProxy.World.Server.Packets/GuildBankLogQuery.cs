namespace HermesProxy.World.Server.Packets;

public class GuildBankLogQuery : ClientPacket
{
	public int Tab;

	public GuildBankLogQuery(WorldPacket packet)
		: base(packet)
	{
	}

	public override void Read()
	{
		Tab = _worldPacket.ReadInt32();
	}
}
