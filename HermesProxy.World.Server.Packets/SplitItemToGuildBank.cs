namespace HermesProxy.World.Server.Packets;

internal class SplitItemToGuildBank : ClientPacket
{
	public WowGuid128 BankGuid;

	public byte BankTab;

	public byte BankSlot;

	public byte? ContainerSlot;

	public byte ContainerItemSlot;

	public uint StackCount;

	public SplitItemToGuildBank(WorldPacket packet)
		: base(packet)
	{
	}

	public override void Read()
	{
		BankGuid = _worldPacket.ReadPackedGuid128();
		BankTab = _worldPacket.ReadUInt8();
		BankSlot = _worldPacket.ReadUInt8();
		ContainerItemSlot = _worldPacket.ReadUInt8();
		StackCount = _worldPacket.ReadUInt32();
		if (_worldPacket.HasBit())
		{
			ContainerSlot = _worldPacket.ReadUInt8();
		}
	}
}
