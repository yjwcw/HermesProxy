using System.Collections.Generic;
using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

public class MailListResult : ServerPacket
{
	public int TotalNumRecords;

	public List<MailListEntry> Mails = new List<MailListEntry>();

	public MailListResult()
		: base(Opcode.SMSG_MAIL_LIST_RESULT)
	{
	}

	public override void Write()
	{
		_worldPacket.WriteInt32(Mails.Count);
		_worldPacket.WriteInt32(TotalNumRecords);
		Mails.ForEach(delegate(MailListEntry p)
		{
			p.Write(_worldPacket);
		});
	}
}
