namespace HermesProxy.World.Server.Packets;

public class AuctionBucketKey
{
	public uint ItemID;

	public ushort ItemLevel;

	public ushort? BattlePetSpeciesID = 0;

	public ushort? SuffixItemNameDescriptionID = 0;

	public AuctionBucketKey()
	{
	}

	public AuctionBucketKey(WorldPacket data)
	{
		data.ResetBitPos();
		ItemID = data.ReadBits<uint>(20);
		if (data.HasBit())
		{
			BattlePetSpeciesID = 0;
		}
		ItemLevel = data.ReadBits<ushort>(11);
		if (data.HasBit())
		{
			SuffixItemNameDescriptionID = 0;
		}
		if (BattlePetSpeciesID.HasValue)
		{
			BattlePetSpeciesID = data.ReadUInt16();
		}
		if (SuffixItemNameDescriptionID.HasValue)
		{
			SuffixItemNameDescriptionID = data.ReadUInt16();
		}
	}

	public void Write(WorldPacket data)
	{
		data.WriteBits(ItemID, 20);
		data.WriteBit(BattlePetSpeciesID.HasValue);
		data.WriteBits(ItemLevel, 11);
		data.WriteBit(SuffixItemNameDescriptionID.HasValue);
		data.FlushBits();
		if (BattlePetSpeciesID.HasValue)
		{
			data.WriteUInt16(BattlePetSpeciesID.Value);
		}
		if (SuffixItemNameDescriptionID.HasValue)
		{
			data.WriteUInt16(SuffixItemNameDescriptionID.Value);
		}
	}
}
