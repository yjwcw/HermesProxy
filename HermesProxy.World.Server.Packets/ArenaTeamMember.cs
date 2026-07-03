using System;
using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

internal struct ArenaTeamMember
{
	public WowGuid128 MemberGUID;

	public bool Online;

	public int Captain;

	public byte Level;

	public Class ClassId;

	public uint WeekGamesPlayed;

	public uint WeekGamesWon;

	public uint SeasonGamesPlayed;

	public uint SeasonGamesWon;

	public uint PersonalRating;

	public string Name;

	public float? dword60;

	public float? dword68;

	public void Write(WorldPacket data)
	{
		data.WritePackedGuid128(MemberGUID);
		data.WriteBool(Online);
		data.WriteInt32(Captain);
		data.WriteUInt8(Level);
		data.WriteUInt8((byte)ClassId);
		data.WriteUInt32(WeekGamesPlayed);
		data.WriteUInt32(WeekGamesWon);
		data.WriteUInt32(SeasonGamesPlayed);
		data.WriteUInt32(SeasonGamesWon);
		data.WriteUInt32(PersonalRating);
		data.WriteBits(Name.GetByteCount(), 6);
		data.WriteBit(dword60.HasValue);
		data.WriteBit(dword68.HasValue);
		data.FlushBits();
		data.WriteString(Name);
		if (dword60.HasValue)
		{
			data.WriteFloat(dword60.Value);
		}
		if (dword68.HasValue)
		{
			data.WriteFloat(dword68.Value);
		}
	}
}
