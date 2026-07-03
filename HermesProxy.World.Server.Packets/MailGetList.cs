namespace HermesProxy.World.Server.Packets;

public class MailGetList : ClientPacket
{
	public WowGuid128 Mailbox;

	public MailGetList(WorldPacket packet)
		: base(packet)
	{
	}

	public override void Read()
	{
		Mailbox = _worldPacket.ReadPackedGuid128();
	}
}
