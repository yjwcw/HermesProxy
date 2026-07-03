using Framework.Constants;
using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

public class LoginSetTimeSpeed : ServerPacket
{
	public uint ServerTime;

	public uint GameTime;

	public float NewSpeed;

	public int ServerTimeHolidayOffset;

	public int GameTimeHolidayOffset;

	public LoginSetTimeSpeed()
		: base(Opcode.SMSG_LOGIN_SET_TIME_SPEED, ConnectionType.Instance)
	{
	}

	public override void Write()
	{
		_worldPacket.WriteUInt32(ServerTime);
		_worldPacket.WriteUInt32(GameTime);
		_worldPacket.WriteFloat(NewSpeed);
		_worldPacket.WriteInt32(ServerTimeHolidayOffset);
		_worldPacket.WriteInt32(GameTimeHolidayOffset);
	}
}
