using System.Collections.Generic;
using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

public class ContactList : ServerPacket
{
	public List<ContactInfo> Contacts;

	public SocialFlag Flags;

	public ContactList()
		: base(Opcode.SMSG_CONTACT_LIST)
	{
		Contacts = new List<ContactInfo>();
	}

	public override void Write()
	{
		_worldPacket.WriteUInt32((uint)Flags);
		_worldPacket.WriteBits(Contacts.Count, 8);
		_worldPacket.FlushBits();
		foreach (ContactInfo contact in Contacts)
		{
			contact.Write(_worldPacket);
		}
	}
}
