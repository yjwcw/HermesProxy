namespace HermesProxy.World.Server.Packets;

public class GuildBankLogEntry
{
	public WowGuid128 PlayerGUID;

	public uint TimeOffset;

	public sbyte EntryType;

	public ulong? Money;

	public int? ItemID;

	public int? Count;

	public sbyte? OtherTab;
}
