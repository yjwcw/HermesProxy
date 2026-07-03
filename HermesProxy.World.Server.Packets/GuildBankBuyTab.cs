namespace HermesProxy.World.Server.Packets;

public class GuildBankBuyTab : ClientPacket
{
	public WowGuid128 BankGuid;

	public byte BankTab;

	public GuildBankBuyTab(WorldPacket packet)
		: base(packet)
	{
	}

	public override void Read()
	{
		BankGuid = _worldPacket.ReadPackedGuid128();
		BankTab = _worldPacket.ReadUInt8();
	}
}
