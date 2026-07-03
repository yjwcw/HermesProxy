using Framework.Constants;
using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

public class TradeStatusPkt : ServerPacket
{
	public bool PartnerIsSameBnetAccount;

	public TradeStatus Status = TradeStatus.Initiated;

	public bool FailureForYou;

	public InventoryResult BagResult;

	public uint ItemID;

	public uint Id;

	public WowGuid128 Partner;

	public WowGuid128 PartnerAccount;

	public byte TradeSlot;

	public int CurrencyType;

	public int CurrencyQuantity;

	public TradeStatusPkt()
		: base(Opcode.SMSG_TRADE_STATUS, ConnectionType.Instance)
	{
	}

	public override void Write()
	{
		_worldPacket.WriteBit(PartnerIsSameBnetAccount);
		_worldPacket.WriteBits(Status, 5);
		switch (Status)
		{
		case TradeStatus.Failed:
			_worldPacket.WriteBit(FailureForYou);
			_worldPacket.WriteInt32((int)BagResult);
			_worldPacket.WriteUInt32(ItemID);
			break;
		case TradeStatus.Initiated:
			_worldPacket.WriteUInt32(Id);
			break;
		case TradeStatus.Proposed:
			_worldPacket.WritePackedGuid128(Partner);
			_worldPacket.WritePackedGuid128(PartnerAccount);
			break;
		case TradeStatus.WrongRealm:
		case TradeStatus.NotOnTaplist:
			_worldPacket.WriteUInt8(TradeSlot);
			break;
		case TradeStatus.CurrencyNotTradable:
		case TradeStatus.NotEnoughCurrency:
			_worldPacket.WriteInt32(CurrencyType);
			_worldPacket.WriteInt32(CurrencyQuantity);
			break;
		default:
			_worldPacket.FlushBits();
			break;
		}
	}
}
