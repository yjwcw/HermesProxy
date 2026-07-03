namespace HermesProxy.World.Server.Packets;

public class GuildBankRemainingWithdrawMoneyQuery : ClientPacket
{
	public GuildBankRemainingWithdrawMoneyQuery(WorldPacket packet)
		: base(packet)
	{
	}

	public override void Read()
	{
	}
}
