using Framework.Constants;
using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

internal class GameObjectCustomAnim : ServerPacket
{
	public WowGuid128 ObjectGUID;

	public uint CustomAnim;

	public bool PlayAsDespawn;

	public GameObjectCustomAnim()
		: base(Opcode.SMSG_GAME_OBJECT_CUSTOM_ANIM, ConnectionType.Instance)
	{
	}

	public override void Write()
	{
		_worldPacket.WritePackedGuid128(ObjectGUID);
		_worldPacket.WriteUInt32(CustomAnim);
		_worldPacket.WriteBit(PlayAsDespawn);
		_worldPacket.FlushBits();
	}
}
