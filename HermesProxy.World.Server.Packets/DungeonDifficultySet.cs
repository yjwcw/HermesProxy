using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

public class DungeonDifficultySet : ServerPacket
{
	public int DifficultyID;

	public DungeonDifficultySet()
		: base(Opcode.SMSG_SET_DUNGEON_DIFFICULTY)
	{
	}

	public override void Write()
	{
		_worldPacket.WriteInt32(DifficultyID);
	}
}
