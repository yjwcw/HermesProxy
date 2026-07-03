using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

internal class GameObjectDespawn : ServerPacket
{
	public WowGuid128 ObjectGUID;

	public GameObjectDespawn()
		: base(Opcode.SMSG_GAME_OBJECT_DESPAWN)
	{
	}

	public override void Write()
	{
		_worldPacket.WritePackedGuid128(ObjectGUID);
	}
}
