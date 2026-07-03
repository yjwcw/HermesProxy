namespace HermesProxy.World.Server.Packets;

public struct LootRequest
{
	public WowGuid128 LootObj;

	public byte LootListID;
}
