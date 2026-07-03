using System;
using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

public class SetTimeZoneInformation : ServerPacket
{
	public string ServerTimeTZ;

	public string GameTimeTZ;

	public SetTimeZoneInformation()
		: base(Opcode.SMSG_SET_TIME_ZONE_INFORMATION)
	{
	}

	public override void Write()
	{
		_worldPacket.WriteBits(ServerTimeTZ.GetByteCount(), 7);
		_worldPacket.WriteBits(GameTimeTZ.GetByteCount(), 7);
		_worldPacket.WriteString(ServerTimeTZ);
		_worldPacket.WriteString(GameTimeTZ);
	}
}
