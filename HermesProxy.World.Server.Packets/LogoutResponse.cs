using Framework.Constants;
using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

public class LogoutResponse : ServerPacket
{
	public int LogoutResult;

	public bool Instant;

	public LogoutResponse()
		: base(Opcode.SMSG_LOGOUT_RESPONSE, ConnectionType.Instance)
	{
	}

	public override void Write()
	{
		_worldPacket.WriteInt32(LogoutResult);
		_worldPacket.WriteBit(Instant);
		_worldPacket.FlushBits();
	}
}
