using System.Collections.Generic;
using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

public class MailQueryNextTimeResult : ServerPacket
{
	public class MailNextTimeEntry
	{
		public WowGuid128 SenderGuid;

		public float TimeLeft;

		public int AltSenderID;

		public sbyte AltSenderType;

		public int StationeryID;
	}

	public float NextMailTime;

	public List<MailNextTimeEntry> Mails = new List<MailNextTimeEntry>();

	public MailQueryNextTimeResult()
		: base(Opcode.SMSG_MAIL_QUERY_NEXT_TIME_RESULT)
	{
	}

	public override void Write()
	{
		_worldPacket.WriteFloat(NextMailTime);
		_worldPacket.WriteInt32(Mails.Count);
		foreach (MailNextTimeEntry mail in Mails)
		{
			_worldPacket.WritePackedGuid128(mail.SenderGuid);
			_worldPacket.WriteFloat(mail.TimeLeft);
			_worldPacket.WriteInt32(mail.AltSenderID);
			_worldPacket.WriteInt8(mail.AltSenderType);
			_worldPacket.WriteInt32(mail.StationeryID);
		}
	}
}
