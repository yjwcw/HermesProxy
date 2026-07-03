using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

public class BuyItem : ClientPacket
{
	public WowGuid128 VendorGUID;

	public ItemInstance Item;

	public uint Slot;

	public uint BagSlot;

	public ItemVendorType ItemType;

	public uint Quantity;

	public WowGuid128 ContainerGUID;

	public BuyItem(WorldPacket packet)
		: base(packet)
	{
		Item = new ItemInstance();
	}

	public override void Read()
	{
		VendorGUID = _worldPacket.ReadPackedGuid128();
		ContainerGUID = _worldPacket.ReadPackedGuid128();
		Quantity = _worldPacket.ReadUInt32();
		Slot = _worldPacket.ReadUInt32();
		BagSlot = _worldPacket.ReadUInt32();
		Item.Read(_worldPacket);
		ItemType = (ItemVendorType)_worldPacket.ReadBits<int>(3);
	}
}
