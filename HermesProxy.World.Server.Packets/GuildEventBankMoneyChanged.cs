using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

public class GuildEventBankMoneyChanged : ServerPacket
{
	public ulong Money;

	public GuildEventBankMoneyChanged()
		: base(Opcode.SMSG_GUILD_EVENT_BANK_MONEY_CHANGED)
	{
	}

	public override void Write()
	{
		_worldPacket.WriteUInt64(Money);
	}
}
