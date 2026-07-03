namespace HermesProxy.World.Server.Packets;

public struct AzeriteEssenceData
{
	public uint Index;

	public uint AzeriteEssenceID;

	public uint Rank;

	public bool SlotUnlocked;

	public void Write(WorldPacket data)
	{
		data.WriteUInt32(Index);
		data.WriteUInt32(AzeriteEssenceID);
		data.WriteUInt32(Rank);
		data.WriteBit(SlotUnlocked);
		data.FlushBits();
	}
}
