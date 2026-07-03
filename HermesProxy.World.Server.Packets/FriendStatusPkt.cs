using System;
using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

public class FriendStatusPkt : ServerPacket
{
	public FriendsResult FriendResult;

	public WowGuid128 Guid;

	public WowGuid128 WowAccountGuid;

	public uint VirtualRealmAddress;

	public FriendStatus Status;

	public uint AreaID;

	public uint Level;

	public Class ClassID;

	public string Notes;

	public bool Mobile;

	public FriendStatusPkt()
		: base(Opcode.SMSG_FRIEND_STATUS)
	{
	}

	public override void Write()
	{
		_worldPacket.WriteUInt8((byte)FriendResult);
		_worldPacket.WritePackedGuid128(Guid);
		_worldPacket.WritePackedGuid128(WowAccountGuid);
		_worldPacket.WriteUInt32(VirtualRealmAddress);
		_worldPacket.WriteUInt8((byte)Status);
		_worldPacket.WriteUInt32(AreaID);
		_worldPacket.WriteUInt32(Level);
		_worldPacket.WriteUInt32((uint)ClassID);
		_worldPacket.WriteBits(Notes.GetByteCount(), 10);
		_worldPacket.WriteBit(Mobile);
		_worldPacket.FlushBits();
		_worldPacket.WriteString(Notes);
	}
}
