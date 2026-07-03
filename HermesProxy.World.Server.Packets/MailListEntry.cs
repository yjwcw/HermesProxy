using System;
using System.Collections.Generic;
using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

public class MailListEntry
{
	public int MailID;

	public MailType SenderType;

	public WowGuid128 SenderCharacter;

	public uint? AltSenderID;

	public ulong Cod;

	public int StationeryID;

	public ulong SentMoney;

	public uint Flags;

	public float DaysLeft;

	public int MailTemplateID;

	public string Subject = "";

	public string Body = "";

	public uint ItemTextId;

	public List<MailAttachedItem> Attachments = new List<MailAttachedItem>();

	public void Write(WorldPacket data)
	{
		data.WriteInt32(MailID);
		data.WriteUInt8((byte)SenderType);
		data.WriteUInt64(Cod);
		data.WriteInt32(StationeryID);
		data.WriteUInt64(SentMoney);
		data.WriteUInt32(Flags);
		data.WriteFloat(DaysLeft);
		data.WriteInt32(MailTemplateID);
		data.WriteInt32(Attachments.Count);
		data.WriteBit(SenderCharacter != null);
		data.WriteBit(AltSenderID.HasValue);
		data.WriteBits(Subject.GetByteCount(), 8);
		data.WriteBits(Body.GetByteCount(), 13);
		data.FlushBits();
		Attachments.ForEach(delegate(MailAttachedItem p)
		{
			p.Write(data);
		});
		if (SenderCharacter != null)
		{
			data.WritePackedGuid128(SenderCharacter);
		}
		if (AltSenderID.HasValue)
		{
			data.WriteUInt32(AltSenderID.Value);
		}
		data.WriteString(Subject);
		data.WriteString(Body);
	}
}
