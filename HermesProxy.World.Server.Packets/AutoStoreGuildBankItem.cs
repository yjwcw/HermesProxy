namespace HermesProxy.World.Server.Packets;

internal class AutoStoreGuildBankItem : ClientPacket
{
	public WowGuid128 BankGuid;

	public byte BankTab;

	public byte BankSlot;

	public AutoStoreGuildBankItem(WorldPacket packet)
		: base(packet)
	{
	}

	public override void Read()
	{
		BankGuid = _worldPacket.ReadPackedGuid128();
		BankTab = _worldPacket.ReadUInt8();
		BankSlot = _worldPacket.ReadUInt8();
	}
}
