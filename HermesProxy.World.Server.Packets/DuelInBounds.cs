using Framework.Constants;
using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

public class DuelInBounds : ServerPacket
{
	public DuelInBounds()
		: base(Opcode.SMSG_DUEL_IN_BOUNDS, ConnectionType.Instance)
	{
	}

	public override void Write()
	{
	}
}
