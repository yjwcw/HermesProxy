using System;
using System.Collections.Generic;
using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

public class GossipMessagePkt : ServerPacket
{
	public List<ClientGossipOption> GossipOptions = new List<ClientGossipOption>();

	public int FriendshipFactionID;

	public WowGuid128 GossipGUID;

	public List<ClientGossipQuest> GossipQuests = new List<ClientGossipQuest>();

	public int TextID;

	public int GossipID;

	public GossipMessagePkt()
		: base(Opcode.SMSG_GOSSIP_MESSAGE)
	{
	}

	public override void Write()
	{
		_worldPacket.WritePackedGuid128(GossipGUID);
		_worldPacket.WriteInt32(GossipID);
		_worldPacket.WriteInt32(FriendshipFactionID);
		_worldPacket.WriteInt32(TextID);
		_worldPacket.WriteInt32(GossipOptions.Count);
		_worldPacket.WriteInt32(GossipQuests.Count);
		foreach (ClientGossipOption gossipOption in GossipOptions)
		{
			_worldPacket.WriteInt32(gossipOption.OptionIndex);
			_worldPacket.WriteUInt8(gossipOption.OptionIcon);
			_worldPacket.WriteUInt8(gossipOption.OptionFlags);
			_worldPacket.WriteInt32(gossipOption.OptionCost);
			if (ModernVersion.AddedInVersion(9, 2, 0, 1, 14, 1, 2, 5, 3))
			{
				_worldPacket.WriteUInt32(gossipOption.Language);
			}
			_worldPacket.WriteBits(gossipOption.Text.GetByteCount(), 12);
			_worldPacket.WriteBits(gossipOption.Confirm.GetByteCount(), 12);
			_worldPacket.WriteBits((byte)gossipOption.Status, 2);
			_worldPacket.WriteBit(gossipOption.SpellID.HasValue);
			_worldPacket.FlushBits();
			gossipOption.Treasure.Write(_worldPacket);
			_worldPacket.WriteString(gossipOption.Text);
			_worldPacket.WriteString(gossipOption.Confirm);
			if (gossipOption.SpellID.HasValue)
			{
				_worldPacket.WriteInt32(gossipOption.SpellID.Value);
			}
		}
		foreach (ClientGossipQuest gossipQuest in GossipQuests)
		{
			gossipQuest.Write(_worldPacket);
		}
	}
}
