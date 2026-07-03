using System;
using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

public class GuildRosterMemberData
{
	public WowGuid128 Guid;

	public long WeeklyXP;

	public long TotalXP;

	public int RankID;

	public int AreaID;

	public int PersonalAchievementPoints = -1;

	public int GuildReputation = -1;

	public int GuildRepToCap;

	public float LastSave;

	public string Name;

	public uint VirtualRealmAddress;

	public string Note;

	public string OfficerNote;

	public byte Status;

	public byte Level;

	public Class ClassID;

	public Gender SexID;

	public bool Authenticated;

	public bool SorEligible;

	public GuildRosterProfessionData[] Profession = new GuildRosterProfessionData[2];

	public void Write(WorldPacket data)
	{
		data.WritePackedGuid128(Guid);
		data.WriteInt32(RankID);
		data.WriteInt32(AreaID);
		data.WriteInt32(PersonalAchievementPoints);
		data.WriteInt32(GuildReputation);
		data.WriteFloat(LastSave);
		for (byte b = 0; b < 2; b++)
		{
			Profession[b].Write(data);
		}
		data.WriteUInt32(VirtualRealmAddress);
		data.WriteUInt8(Status);
		data.WriteUInt8(Level);
		data.WriteUInt8((byte)ClassID);
		data.WriteUInt8((byte)SexID);
		data.WriteBits(Name.GetByteCount(), 6);
		data.WriteBits(Note.GetByteCount(), 8);
		data.WriteBits(OfficerNote.GetByteCount(), 8);
		data.WriteBit(Authenticated);
		data.WriteBit(SorEligible);
		data.WriteString(Name);
		data.WriteString(Note);
		data.WriteString(OfficerNote);
	}
}
