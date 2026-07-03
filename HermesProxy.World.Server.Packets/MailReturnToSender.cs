namespace HermesProxy.World.Server.Packets;

public class MailReturnToSender : ClientPacket
{
	public uint MailID;

	public WowGuid128 SenderGUID;

	public MailReturnToSender(WorldPacket packet)
		: base(packet)
	{
	}

	public override void Read()
	{
		MailID = _worldPacket.ReadUInt32();
		SenderGUID = _worldPacket.ReadPackedGuid128();
	}
}
