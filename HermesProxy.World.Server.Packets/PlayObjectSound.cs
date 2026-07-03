using Framework.GameMath;
using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

internal class PlayObjectSound : ServerPacket
{
	public uint SoundEntryID;

	public WowGuid128 SourceObjectGUID;

	public WowGuid128 TargetObjectGUID;

	public Vector3 Position;

	public int BroadcastTextID;

	public PlayObjectSound()
		: base(Opcode.SMSG_PLAY_OBJECT_SOUND)
	{
	}

	public override void Write()
	{
		_worldPacket.WriteUInt32(SoundEntryID);
		_worldPacket.WritePackedGuid128(SourceObjectGUID);
		_worldPacket.WritePackedGuid128(TargetObjectGUID);
		_worldPacket.WriteVector3(Position);
		if (ModernVersion.AddedInVersion(9, 0, 1, 1, 14, 0, 2, 5, 1))
		{
			_worldPacket.WriteInt32(BroadcastTextID);
		}
	}
}
