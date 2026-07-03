using Framework.Constants;
using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

internal class ResumeComms : ServerPacket
{
	public ResumeComms(ConnectionType connection)
		: base(Opcode.SMSG_RESUME_COMMS, connection)
	{
	}

	public override void Write()
	{
	}
}
