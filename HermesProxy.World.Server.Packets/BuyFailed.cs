using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

public class BuyFailed : ServerPacket
{
	public WowGuid128 VendorGUID;

	public uint Slot;

	public BuyResult Reason;

	public BuyFailed()
		: base(Opcode.SMSG_BUY_FAILED)
	{
	}

	public override void Write()
	{
		_worldPacket.WritePackedGuid128(VendorGUID);
		_worldPacket.WriteUInt32(Slot);
		_worldPacket.WriteUInt8((byte)Reason);
	}
}
