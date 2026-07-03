using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

public class LogoutComplete : ServerPacket
{
	public LogoutComplete()
		: base(Opcode.SMSG_LOGOUT_COMPLETE)
	{
	}

	public override void Write()
	{
	}
}
