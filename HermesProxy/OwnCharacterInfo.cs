using HermesProxy.World;

namespace HermesProxy;

public class OwnCharacterInfo : PlayerCache
{
	public WowGuid128 AccountId;

	public WowGuid128 CharacterGuid;

	public Realm Realm;

	public ulong LastLoginUnixSec;
}
