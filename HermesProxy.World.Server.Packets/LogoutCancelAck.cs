using Framework.Constants;
using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

public class LogoutCancelAck : ServerPacket
{
	public LogoutCancelAck()
		: base(Opcode.SMSG_LOGOUT_CANCEL_ACK, ConnectionType.Instance)
	{
	}

	public override void Write()
	{
	}
}
