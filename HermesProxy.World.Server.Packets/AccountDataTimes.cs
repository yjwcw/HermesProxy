using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

public class AccountDataTimes : ServerPacket
{
	public WowGuid128 PlayerGuid;

	public long ServerTime;

	public long[] AccountTimes;

	public AccountDataTimes()
		: base(Opcode.SMSG_ACCOUNT_DATA_TIMES)
	{
	}

	public override void Write()
	{
		_worldPacket.WritePackedGuid128(PlayerGuid);
		_worldPacket.WriteInt64(ServerTime);
		long[] accountTimes = AccountTimes;
		foreach (long data in accountTimes)
		{
			_worldPacket.WriteInt64(data);
		}
	}
}
