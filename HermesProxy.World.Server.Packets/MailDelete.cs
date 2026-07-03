namespace HermesProxy.World.Server.Packets;

public class MailDelete : ClientPacket
{
	public uint MailID;

	public int DeleteReason;

	public MailDelete(WorldPacket packet)
		: base(packet)
	{
	}

	public override void Read()
	{
		MailID = _worldPacket.ReadUInt32();
		DeleteReason = _worldPacket.ReadInt32();
	}
}
