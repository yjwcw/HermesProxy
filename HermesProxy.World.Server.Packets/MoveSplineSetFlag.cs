using Framework.Constants;
using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

public class MoveSplineSetFlag : ServerPacket
{
	public WowGuid128 MoverGUID;

	public MoveSplineSetFlag(Opcode opcode)
		: base(opcode, ConnectionType.Instance)
	{
	}

	public override void Write()
	{
		_worldPacket.WritePackedGuid128(MoverGUID);
	}
}
