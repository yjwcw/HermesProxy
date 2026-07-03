using Framework.Constants;
using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

public class STextEmote : ServerPacket
{
	public WowGuid128 SourceGUID;

	public WowGuid128 SourceAccountGUID;

	public WowGuid128 TargetGUID;

	public int SoundIndex = -1;

	public int EmoteID;

	public STextEmote()
		: base(Opcode.SMSG_TEXT_EMOTE, ConnectionType.Instance)
	{
	}

	public override void Write()
	{
		_worldPacket.WritePackedGuid128(SourceGUID);
		_worldPacket.WritePackedGuid128(SourceAccountGUID);
		_worldPacket.WriteInt32(EmoteID);
		_worldPacket.WriteInt32(SoundIndex);
		_worldPacket.WritePackedGuid128(TargetGUID);
	}
}
