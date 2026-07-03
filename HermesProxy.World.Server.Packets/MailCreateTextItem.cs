namespace HermesProxy.World.Server.Packets;

public class MailCreateTextItem : ClientPacket
{
	public WowGuid128 Mailbox;

	public uint MailID;

	public MailCreateTextItem(WorldPacket packet)
		: base(packet)
	{
	}

	public override void Read()
	{
		Mailbox = _worldPacket.ReadPackedGuid128();
		MailID = _worldPacket.ReadUInt32();
	}
}
