using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

internal class LootMoneyNotify : ServerPacket
{
	public ulong Money;

	public ulong MoneyMod;

	public bool SoleLooter;

	public LootMoneyNotify()
		: base(Opcode.SMSG_LOOT_MONEY_NOTIFY)
	{
	}

	public override void Write()
	{
		_worldPacket.WriteUInt64(Money);
		_worldPacket.WriteUInt64(MoneyMod);
		_worldPacket.WriteBit(SoleLooter);
		_worldPacket.FlushBits();
	}
}
