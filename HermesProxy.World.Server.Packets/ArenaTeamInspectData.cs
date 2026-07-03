namespace HermesProxy.World.Server.Packets;

public class ArenaTeamInspectData
{
	public WowGuid128 TeamGuid = WowGuid128.Empty;

	public int TeamRating;

	public int TeamGamesPlayed;

	public int TeamGamesWon;

	public int PersonalGamesPlayed;

	public int PersonalRating;

	public void Write(WorldPacket data)
	{
		data.WritePackedGuid128(TeamGuid);
		data.WriteInt32(TeamRating);
		data.WriteInt32(TeamGamesPlayed);
		data.WriteInt32(TeamGamesWon);
		data.WriteInt32(PersonalGamesPlayed);
		data.WriteInt32(PersonalRating);
	}
}
