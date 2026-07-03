using HermesProxy.World;

namespace HermesProxy;

public class TradeSession
{
	public static uint GlobalTradeIdCounter;

	public uint TradeId;

	public WowGuid128 Partner;

	public WowGuid128 PartnerAccount;

	public uint ClientStateIndex = 1u;

	public uint ServerStateIndex = 1u;
}
