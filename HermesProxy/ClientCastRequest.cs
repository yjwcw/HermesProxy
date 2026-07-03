using HermesProxy.World;

namespace HermesProxy;

public class ClientCastRequest
{
	public bool HasStarted;

	public uint SpellId;

	public uint SpellXSpellVisualId;

	public long Timestamp;

	public WowGuid128 ClientGUID;

	public WowGuid128 ServerGUID;

	public WowGuid128 ItemGUID;
}
