using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

public class BattlegroundInit : ServerPacket
{
	public uint Milliseconds;

	public ushort BattlegroundPoints;

	public BattlegroundInit()
		: base(Opcode.SMSG_BATTLEGROUND_INIT)
	{
	}

	public override void Write()
	{
		_worldPacket.WriteUInt32(Milliseconds);
		_worldPacket.WriteUInt16(BattlegroundPoints);
	}
}
