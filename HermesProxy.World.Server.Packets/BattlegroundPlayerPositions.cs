using System.Collections.Generic;
using Framework.Constants;
using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

internal class BattlegroundPlayerPositions : ServerPacket
{
	public List<BattlegroundPlayerPosition> FlagCarriers = new List<BattlegroundPlayerPosition>();

	public BattlegroundPlayerPositions()
		: base(Opcode.SMSG_BATTLEGROUND_PLAYER_POSITIONS, ConnectionType.Instance)
	{
	}

	public override void Write()
	{
		_worldPacket.WriteInt32(FlagCarriers.Count);
		foreach (BattlegroundPlayerPosition flagCarrier in FlagCarriers)
		{
			flagCarrier.Write(_worldPacket);
		}
	}
}
