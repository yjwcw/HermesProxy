using System;
using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

public class ContactInfo
{
	public WowGuid128 Guid;

	public WowGuid128 WowAccountGuid;

	public uint VirtualRealmAddr;

	public uint NativeRealmAddr;

	public SocialFlag TypeFlags;

	public FriendStatus Status;

	public uint AreaID;

	public uint Level;

	public Class ClassID;

	public bool Mobile;

	public string Note = "";

	public void Write(WorldPacket data)
	{
		data.WritePackedGuid128(Guid);
		data.WritePackedGuid128(WowAccountGuid);
		data.WriteUInt32(VirtualRealmAddr);
		data.WriteUInt32(NativeRealmAddr);
		data.WriteUInt32((uint)TypeFlags);
		data.WriteUInt8((byte)Status);
		data.WriteUInt32(AreaID);
		data.WriteUInt32(Level);
		data.WriteUInt32((uint)ClassID);
		data.WriteBits(Note.GetByteCount(), 10);
		data.WriteBit(Mobile);
		data.FlushBits();
		data.WriteString(Note);
	}
}
