using System.Collections.Generic;

namespace HermesProxy.World.Server.Packets;

public class SendMail : ClientPacket
{
	public struct MailAttachment
	{
		public byte AttachPosition;

		public WowGuid128 ItemGUID;
	}

	public WowGuid128 Mailbox;

	public int StationeryID;

	public long SendMoney;

	public long Cod;

	public string Target;

	public string Subject;

	public string Body;

	public List<MailAttachment> Attachments = new List<MailAttachment>();

	public SendMail(WorldPacket packet)
		: base(packet)
	{
	}

	public override void Read()
	{
		Mailbox = _worldPacket.ReadPackedGuid128();
		StationeryID = _worldPacket.ReadInt32();
		SendMoney = _worldPacket.ReadInt64();
		Cod = _worldPacket.ReadInt64();
		uint length = _worldPacket.ReadBits<uint>(9);
		uint length2 = _worldPacket.ReadBits<uint>(9);
		uint length3 = _worldPacket.ReadBits<uint>(11);
		uint num = _worldPacket.ReadBits<uint>(5);
		Target = _worldPacket.ReadString(length);
		Subject = _worldPacket.ReadString(length2);
		Body = _worldPacket.ReadString(length3);
		for (int i = 0; i < num; i++)
		{
			MailAttachment mailAttachment = default(MailAttachment);
			mailAttachment.AttachPosition = _worldPacket.ReadUInt8();
			mailAttachment.ItemGUID = _worldPacket.ReadPackedGuid128();
			MailAttachment mailAttachment2 = mailAttachment;
			Attachments.Add(mailAttachment2);
		}
	}
}
