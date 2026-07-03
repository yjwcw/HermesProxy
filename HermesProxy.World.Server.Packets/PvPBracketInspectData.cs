namespace HermesProxy.World.Server.Packets;

public class PvPBracketInspectData
{
	public byte Bracket;

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
		data.WriteInt32(WeeklyBestWinPvpTierID);
		data.WriteBool(Disqualified);
	}
}
