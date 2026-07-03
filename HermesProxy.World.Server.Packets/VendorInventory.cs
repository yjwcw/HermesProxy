using System.Collections.Generic;
using Framework.Constants;
using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

public class VendorInventory : ServerPacket
{
	public byte Reason;

	public List<VendorItem> Items = new List<VendorItem>();

	public WowGuid128 VendorGUID;

	public VendorInventory()
		: base(Opcode.SMSG_VENDOR_INVENTORY, ConnectionType.Instance)
	{
	}

	public override void Write()
	{
		_worldPacket.WritePackedGuid128(VendorGUID);
		_worldPacket.WriteUInt8(Reason);
		_worldPacket.WriteInt32(Items.Count);
		foreach (VendorItem item in Items)
		{
			item.Write(_worldPacket);
		}
	}
}
