using System;
using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

public class ChannelNotifyJoined : ServerPacket
{
	public string ChannelWelcomeMsg = "";

	public int ChatChannelID;

	public ulong InstanceID;

	public ChannelFlags ChannelFlags;

	public string Channel = "";

	public WowGuid128 ChannelGUID;

	public ChannelNotifyJoined()
		: base(Opcode.SMSG_CHANNEL_NOTIFY_JOINED)
	{
	}

	public override void Write()
	{
		_worldPacket.WriteBits(Channel.GetByteCount(), 7);
		_worldPacket.WriteBits(ChannelWelcomeMsg.GetByteCount(), 11);
		_worldPacket.WriteUInt32((uint)ChannelFlags);
		_worldPacket.WriteInt32(ChatChannelID);
		_worldPacket.WriteUInt64(InstanceID);
		_worldPacket.WritePackedGuid128(ChannelGUID);
		_worldPacket.WriteString(Channel);
		_worldPacket.WriteString(ChannelWelcomeMsg);
	}
}
