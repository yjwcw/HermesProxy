namespace HermesProxy.World.Server.Packets;

internal class MoveGuildBankItem : ClientPacket
{
	public WowGuid128 BankGuid;

	public byte BankTab1;

	public byte BankSlot1;

	public byte BankTab2;

	public byte BankSlot2;

	public MoveGuildBankItem(WorldPacket packet)
		: base(packet)
	{
	}

	public override void Read()
	{
		BankGuid = _worldPacket.ReadPackedGuid128();
		BankTab1 = _worldPacket.ReadUInt8();
		BankSlot1 = _worldPacket.ReadUInt8();
		BankTab2 = _worldPacket.ReadUInt8();
		BankSlot2 = _worldPacket.ReadUInt8();
	}
}
