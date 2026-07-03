using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

internal class PlaySound : ServerPacket
{
	public uint SoundEntryID;

	public WowGuid128 SourceObjectGuid;

	public int BroadcastTextId;

	public PlaySound()
		: base(Opcode.SMSG_PLAY_SOUND)
	{
	}

	public override void Write()
	{
		_worldPacket.WriteUInt32(SoundEntryID);
		_worldPacket.WritePackedGuid128(SourceObjectGuid);
		if (ModernVersion.AddedInVersion(9, 0, 1, 1, 14, 0, 2, 5, 1))
		{
			_worldPacket.WriteInt32(BroadcastTextId);
		}
	}
}
