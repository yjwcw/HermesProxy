using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

public class InventoryChangeFailure : ServerPacket
{
	public InventoryResult BagResult;

	public byte ContainerBSlot;

	public WowGuid128 SrcContainer;

	public WowGuid128 DstContainer;

	public int SrcSlot;

	public int LimitCategory;

	public int Level;

	public WowGuid128[] Item = new WowGuid128[2];

	public InventoryChangeFailure()
		: base(Opcode.SMSG_INVENTORY_CHANGE_FAILURE)
	{
	}

	public override void Write()
	{
		_worldPacket.WriteInt8((sbyte)BagResult);
		_worldPacket.WritePackedGuid128(Item[0]);
		_worldPacket.WritePackedGuid128(Item[1]);
		_worldPacket.WriteUInt8(ContainerBSlot);
		switch (BagResult)
		{
		case InventoryResult.CantEquipLevel:
		case InventoryResult.PurchaseLevelTooLow:
			_worldPacket.WriteInt32(Level);
			break;
		case InventoryResult.EventAutoEquipBindConfirm:
			_worldPacket.WritePackedGuid128(SrcContainer);
			_worldPacket.WriteInt32(SrcSlot);
			_worldPacket.WritePackedGuid128(DstContainer);
			break;
		case InventoryResult.ItemMaxLimitCategoryCountExceeded:
		case InventoryResult.ItemMaxLimitCategorySocketedExceeded:
		case InventoryResult.ItemMaxLimitCategoryEquippedExceeded:
			_worldPacket.WriteInt32(LimitCategory);
			break;
		}
	}
}
