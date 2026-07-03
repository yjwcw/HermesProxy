namespace HermesProxy.World.Server.Packets;

public struct PVPBracketData
{
	public int Rating;

	public int Rank;

	public int WeeklyPlayed;

	public int WeeklyWon;

	public int SeasonPlayed;

	public int SeasonWon;

	public int WeeklyBestRating;

	public int SeasonBestRating;

	public int PvpTierID;

	public int WeeklyBestWinPvpTierID;

	public int Unused1;

	public int Unused2;

	public byte Bracket;

	public bool Disqualified;

	public void Write(WorldPacket data)
	{
		data.WriteUInt8(Bracket);
		data.WriteInt32(Rating);
		data.WriteInt32(Rank);
		data.WriteInt32(WeeklyPlayed);
		data.WriteInt32(WeeklyWon);
		data.WriteInt32(SeasonPlayed);
		data.WriteInt32(SeasonWon);
		data.WriteInt32(WeeklyBestRating);
		data.WriteInt32(SeasonBestRating);
		data.WriteInt32(PvpTierID);
		if (ModernVersion.AddedInVersion(9, 1, 0, 1, 14, 0, 2, 5, 2))
		{
			data.WriteInt32(WeeklyBestWinPvpTierID);
		}
		if (ModernVersion.AddedInVersion(9, 1, 5, 1, 14, 1, 2, 5, 3))
		{
			data.WriteInt32(Unused1);
			data.WriteInt32(Unused2);
		}
		data.WriteBit(Disqualified);
		data.FlushBits();
	}
}
