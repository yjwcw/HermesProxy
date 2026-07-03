using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

public class SellResponse : ServerPacket
{
	public WowGuid128 VendorGUID;

	public WowGuid128 ItemGUID;

	public byte Reason;

	public SellResponse()
		: base(Opcode.SMSG_SELL_RESPONSE)
	{
	}

	public override void Write()
	{
		_worldPacket.WritePackedGuid128(VendorGUID);
		_worldPacket.WritePackedGuid128(ItemGUID);
		_worldPacket.WriteUInt8(Reason);
	}
}
