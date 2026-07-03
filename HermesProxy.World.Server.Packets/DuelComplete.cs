using Framework.Constants;
using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

public class DuelComplete : ServerPacket
{
	public bool Started;

	public DuelComplete()
		: base(Opcode.SMSG_DUEL_COMPLETE, ConnectionType.Instance)
	{
	}

	public override void Write()
	{
		_worldPacket.WriteBit(Started);
		_worldPacket.FlushBits();
	}
}
