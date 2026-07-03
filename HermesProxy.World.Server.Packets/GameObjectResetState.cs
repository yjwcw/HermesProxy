using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

internal class GameObjectResetState : ServerPacket
{
	public WowGuid128 ObjectGUID;

	public GameObjectResetState()
		: base(Opcode.SMSG_GAME_OBJECT_RESET_STATE)
	{
	}

	public override void Write()
	{
		_worldPacket.WritePackedGuid128(ObjectGUID);
	}
}
