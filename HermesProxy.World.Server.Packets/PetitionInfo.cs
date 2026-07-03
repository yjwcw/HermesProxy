using System;
using Framework.Collections;

namespace HermesProxy.World.Server.Packets;

public class PetitionInfo
{
	public uint PetitionID;

	public WowGuid128 Petitioner;

	public string Title;

	public string BodyText;

	public uint MinSignatures;

	public uint MaxSignatures;

	public int DeadLine;

	public int IssueDate;

	public int AllowedGuildID;

	public int AllowedClasses;

	public int AllowedRaces;

	public short AllowedGender;

	public int AllowedMinLevel;

	public int AllowedMaxLevel;

	public int NumChoices;

	public int StaticType;

	public uint Muid;

	public StringArray Choicetext = new StringArray(10);

	public void Write(WorldPacket data)
	{
		data.WriteUInt32(PetitionID);
		data.WritePackedGuid128(Petitioner);
		data.WriteUInt32(MinSignatures);
		data.WriteUInt32(MaxSignatures);
		data.WriteInt32(DeadLine);
		data.WriteInt32(IssueDate);
		data.WriteInt32(AllowedGuildID);
		data.WriteInt32(AllowedClasses);
		data.WriteInt32(AllowedRaces);
		data.WriteInt16(AllowedGender);
		data.WriteInt32(AllowedMinLevel);
		data.WriteInt32(AllowedMaxLevel);
		data.WriteInt32(NumChoices);
		data.WriteInt32(StaticType);
		data.WriteUInt32(Muid);
		data.WriteBits(Title.GetByteCount(), 7);
		data.WriteBits(BodyText.GetByteCount(), 12);
		for (byte b = 0; b < Choicetext.Length; b++)
		{
			data.WriteBits(Choicetext[b].GetByteCount(), 6);
		}
		data.FlushBits();
		for (byte b2 = 0; b2 < Choicetext.Length; b2++)
		{
			data.WriteString(Choicetext[b2]);
		}
		data.WriteString(Title);
		data.WriteString(BodyText);
	}
}
