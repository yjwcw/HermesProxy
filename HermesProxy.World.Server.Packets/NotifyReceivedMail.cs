using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

public class NotifyReceivedMail : ServerPacket
{
	public float Delay;

	public NotifyReceivedMail()
		: base(Opcode.SMSG_NOTIFY_RECEIVED_MAIL)
	{
	}

	public override void Write()
	{
		_worldPacket.WriteFloat(Delay);
	}
}
