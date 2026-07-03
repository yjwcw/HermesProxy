using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

internal class ReadItemResultFailed : ServerPacket
{
	public WowGuid128 ItemGUID;

	public uint Delay;

	public byte Subcode;

	public ReadItemResultFailed()
		: base(Opcode.SMSG_READ_ITEM_RESULT_FAILED)
	{
	}

	public override void Write()
	{
		_worldPacket.WritePackedGuid128(ItemGUID);
		_worldPacket.WriteUInt32(Delay);
		_worldPacket.WriteBits(Subcode, 2);
		_worldPacket.FlushBits();
	}
}
