namespace HermesProxy.World.Server.Packets;

public class GuildBankQueryTab : ClientPacket
{
	public WowGuid128 BankGuid;

	public byte Tab;

	public bool FullUpdate;

	public GuildBankQueryTab(WorldPacket packet)
		: base(packet)
	{
	}

	public override void Read()
	{
		BankGuid = _worldPacket.ReadPackedGuid128();
		Tab = _worldPacket.ReadUInt8();
		FullUpdate = _worldPacket.HasBit();
	}
}
