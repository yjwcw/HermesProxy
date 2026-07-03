using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

public class BattlefieldStatusNeedConfirmation : ServerPacket
{
	public BattlefieldStatusHeader Hdr = new BattlefieldStatusHeader();

	public uint Mapid;

	public uint Timeout;

	public byte Role;

	public BattlefieldStatusNeedConfirmation()
		: base(Opcode.SMSG_BATTLEFIELD_STATUS_NEED_CONFIRMATION)
	{
	}

	public override void Write()
	{
		Hdr.Write(_worldPacket);
		_worldPacket.WriteUInt32(Mapid);
		_worldPacket.WriteUInt32(Timeout);
		_worldPacket.WriteUInt8(Role);
	}
}
