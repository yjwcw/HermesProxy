using System;
using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

public class ChannelNotifyLeft : ServerPacket
{
	public string Channel;

	public int ChatChannelID;

	public bool Suspended;

	public ChannelNotifyLeft()
		: base(Opcode.SMSG_CHANNEL_NOTIFY_LEFT)
	{
	}

	public override void Write()
	{
		_worldPacket.WriteBits(Channel.GetByteCount(), 7);
		_worldPacket.WriteBit(Suspended);
		_worldPacket.WriteInt32(ChatChannelID);
		_worldPacket.WriteString(Channel);
	}
}
