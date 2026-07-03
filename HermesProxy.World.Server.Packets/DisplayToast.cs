using Framework.Constants;
using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

public class DisplayToast : ServerPacket
{
	public ulong Quantity;

	public byte DisplayToastMethod = 16;

	public uint QuestID;

	public bool Mailed;

	public byte Type;

	public bool BonusRoll;

	public ItemInstance ItemReward = new ItemInstance();

	public uint SpecializationID;

	public uint ItemQuantity;

	public uint CurrencyID;

	public DisplayToast()
		: base(Opcode.SMSG_DISPLAY_TOAST, ConnectionType.Instance)
	{
	}

	public override void Write()
	{
		_worldPacket.WriteUInt64(Quantity);
		_worldPacket.WriteUInt8(DisplayToastMethod);
		_worldPacket.WriteUInt32(QuestID);
		_worldPacket.WriteBit(Mailed);
		_worldPacket.WriteBits(Type, 2);
		if (Type == 0)
		{
			_worldPacket.WriteBit(BonusRoll);
			_worldPacket.FlushBits();
			ItemReward.Write(_worldPacket);
			_worldPacket.WriteUInt32(SpecializationID);
			_worldPacket.WriteUInt32(ItemQuantity);
		}
		else
		{
			_worldPacket.FlushBits();
		}
		if (Type == 1)
		{
			_worldPacket.WriteUInt32(CurrencyID);
		}
	}
}
