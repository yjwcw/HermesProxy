using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

internal class UpdateLastInstance : ServerPacket
{
	public uint MapID;

	public UpdateLastInstance()
		: base(Opcode.SMSG_UPDATE_LAST_INSTANCE)
	{
	}

	public override void Write()
	{
		_worldPacket.WriteUInt32(MapID);
	}
}
