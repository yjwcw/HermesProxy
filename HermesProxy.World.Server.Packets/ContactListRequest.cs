using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

public class ContactListRequest : ClientPacket
{
	public SocialFlag Flags;

	public ContactListRequest(WorldPacket packet)
		: base(packet)
	{
	}

	public override void Read()
	{
		Flags = (SocialFlag)_worldPacket.ReadUInt32();
	}
}
