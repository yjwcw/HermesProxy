namespace HermesProxy.World.Server.Packets;

public class GuildBankAtivate : ClientPacket
{
	public WowGuid128 BankGuid;

	public bool FullUpdate;

	public GuildBankAtivate(WorldPacket packet)
		: base(packet)
	{
	}

	public override void Read()
	{
		BankGuid = _worldPacket.ReadPackedGuid128();
		FullUpdate = _worldPacket.HasBit();
	}
}
