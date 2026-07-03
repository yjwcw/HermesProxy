using Framework.Constants;
using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

public class MoveSetFlag : ServerPacket
{
	public WowGuid128 MoverGUID;

	public uint MoveCounter;

	public MoveSetFlag(Opcode opcode)
		: base(opcode, ConnectionType.Instance)
	{
	}

	public override void Write()
	{
		_worldPacket.WritePackedGuid128(MoverGUID);
		_worldPacket.WriteUInt32(MoveCounter);
	}
}
