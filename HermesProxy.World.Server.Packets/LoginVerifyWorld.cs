using Framework.Constants;
using Framework.GameMath;
using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

public class LoginVerifyWorld : ServerPacket
{
	public uint MapID;

	public Position Pos;

	public uint Reason;

	public LoginVerifyWorld()
		: base(Opcode.SMSG_LOGIN_VERIFY_WORLD, ConnectionType.Instance)
	{
	}

	public override void Write()
	{
		_worldPacket.WriteUInt32(MapID);
		_worldPacket.WriteFloat(Pos.X);
		_worldPacket.WriteFloat(Pos.Y);
		_worldPacket.WriteFloat(Pos.Z);
		_worldPacket.WriteFloat(Pos.Orientation);
		_worldPacket.WriteUInt32(Reason);
	}
}
