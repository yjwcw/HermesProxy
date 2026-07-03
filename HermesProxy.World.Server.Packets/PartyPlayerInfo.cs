using System;
using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

public struct PartyPlayerInfo
{
	public WowGuid128 GUID;

	public string Name;

	public string VoiceStateID;

	public Class ClassId;

	public GroupMemberOnlineStatus Status;

	public byte Subgroup;

	public GroupMemberFlags Flags;

	public byte RolesAssigned;

	public bool FromSocialQueue;

	public bool VoiceChatSilenced;

	public void Write(WorldPacket data)
	{
		data.WriteBits(Name.GetByteCount(), 6);
		data.WriteBits(VoiceStateID.GetByteCount() + 1, 6);
		data.WriteBit(FromSocialQueue);
		data.WriteBit(VoiceChatSilenced);
		data.WritePackedGuid128(GUID);
		data.WriteUInt8((byte)Status);
		data.WriteUInt8(Subgroup);
		data.WriteUInt8((byte)Flags);
		data.WriteUInt8(RolesAssigned);
		data.WriteUInt8((byte)ClassId);
		data.WriteString(Name);
		if (!VoiceStateID.IsEmpty())
		{
			data.WriteString(VoiceStateID);
		}
	}
}
