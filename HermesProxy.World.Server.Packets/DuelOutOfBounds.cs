using Framework.Constants;
using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

public class DuelOutOfBounds : ServerPacket
{
	public DuelOutOfBounds()
		: base(Opcode.SMSG_DUEL_OUT_OF_BOUNDS, ConnectionType.Instance)
	{
	}

	public override void Write()
	{
	}
}
