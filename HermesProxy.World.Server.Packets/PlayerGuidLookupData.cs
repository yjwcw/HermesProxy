using System;
using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

public class PlayerGuidLookupData
{
	public bool IsDeleted;

	public WowGuid128 AccountID;

	public WowGuid128 BnetAccountID;

	public WowGuid128 GuidActual;

	public string Name = "";

	public ulong GuildClubMemberID;

	public uint VirtualRealmAddress;

	public Race RaceID;

	public Gender Sex = Gender.None;

	public Class ClassID;

	public byte Level;

	public byte Unused915;

	public DeclinedName DeclinedNames = new DeclinedName();

	public void Write(WorldPacket data)
	{
		data.WriteBit(IsDeleted);
		data.WriteBits(Name.GetByteCount(), 6);
		for (byte b = 0; b < 5; b++)
		{
			data.WriteBits(DeclinedNames.name[b].GetByteCount(), 7);
		}
		data.FlushBits();
		for (byte b2 = 0; b2 < 5; b2++)
		{
			data.WriteString(DeclinedNames.name[b2]);
		}
		data.WritePackedGuid128(AccountID);
		data.WritePackedGuid128(BnetAccountID);
		data.WritePackedGuid128(GuidActual);
		data.WriteUInt64(GuildClubMemberID);
		data.WriteUInt32(VirtualRealmAddress);
		data.WriteUInt8((byte)RaceID);
		data.WriteUInt8((byte)Sex);
		data.WriteUInt8((byte)ClassID);
		data.WriteUInt8(Level);
		data.WriteUInt8(Unused915);
		data.WriteString(Name);
	}
}
