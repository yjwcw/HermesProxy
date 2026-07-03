namespace HermesProxy.World.Server.Packets;

internal class AutoGuildBankItem : ClientPacket
{
	public WowGuid BankGuid;

	public byte BankTab;

	public byte BankSlot;

	public byte? ContainerSlot;

	public byte ContainerItemSlot;

	public AutoGuildBankItem(WorldPacket packet)
		: base(packet)
	{
	}

	public override void Read()
	{
		BankGuid = _worldPacket.ReadPackedGuid128();
		BankTab = _worldPacket.ReadUInt8();
		BankSlot = _worldPacket.ReadUInt8();
		ContainerItemSlot = _worldPacket.ReadUInt8();
		if (_worldPacket.HasBit())
		{
			ContainerSlot = _worldPacket.ReadUInt8();
		}
	}
}
