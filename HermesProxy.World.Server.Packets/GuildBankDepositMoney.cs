namespace HermesProxy.World.Server.Packets;

public class GuildBankDepositMoney : ClientPacket
{
	public WowGuid128 BankGuid;

	public ulong Money;

	public GuildBankDepositMoney(WorldPacket packet)
		: base(packet)
	{
	}

	public override void Read()
	{
		BankGuid = _worldPacket.ReadPackedGuid128();
		Money = _worldPacket.ReadUInt64();
	}
}
