namespace HermesProxy.World.Server.Packets;

public struct CriteriaProgressPkt
{
	public uint Id;

	public ulong Quantity;

	public WowGuid128 Player;

	public uint Flags;

	public long Date;

	public uint TimeFromStart;

	public uint TimeFromCreate;

	public ulong? RafAcceptanceID;

	public void Write(WorldPacket data)
	{
		data.WriteUInt32(Id);
		data.WriteUInt64(Quantity);
		data.WritePackedGuid128(Player);
		data.WritePackedTime(Date);
		data.WriteUInt32(TimeFromStart);
		data.WriteUInt32(TimeFromCreate);
		data.WriteBits(Flags, 4);
		data.WriteBit(RafAcceptanceID.HasValue);
		data.FlushBits();
		if (RafAcceptanceID.HasValue)
		{
			data.WriteUInt64(RafAcceptanceID.Value);
		}
	}
}
