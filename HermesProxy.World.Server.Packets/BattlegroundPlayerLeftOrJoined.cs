using Framework.Constants;
using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

internal class BattlegroundPlayerLeftOrJoined : ServerPacket
{
	public WowGuid128 Guid;

	public BattlegroundPlayerLeftOrJoined(Opcode opcode)
		: base(opcode, ConnectionType.Instance)
	{
	}

	public override void Write()
	{
		_worldPacket.WritePackedGuid128(Guid);
	}
}
