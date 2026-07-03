using System;
using System.Collections.Generic;
using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

internal class PartyInvite : ServerPacket
{
	public bool CanAccept = true;

	public bool MightCRZYou;

	public bool IsXRealm;

	public bool MustBeBNetFriend;

	public bool AllowMultipleRoles;

	public bool QuestSessionActive;

	public ushort Unk1 = 4904;

	public VirtualRealmInfo InviterRealm;

	public WowGuid128 InviterGUID;

	public WowGuid128 InviterBNetAccountId;

	public string InviterName;

	public uint ProposedRoles;

	public int LfgCompletedMask;

	public List<int> LfgSlots = new List<int>();

	public PartyInvite()
		: base(Opcode.SMSG_PARTY_INVITE)
	{
	}

	public override void Write()
	{
		_worldPacket.WriteBit(CanAccept);
		_worldPacket.WriteBit(MightCRZYou);
		_worldPacket.WriteBit(IsXRealm);
		_worldPacket.WriteBit(MustBeBNetFriend);
		_worldPacket.WriteBit(AllowMultipleRoles);
		_worldPacket.WriteBit(QuestSessionActive);
		_worldPacket.WriteBits(InviterName.GetByteCount(), 6);
		InviterRealm.Write(_worldPacket);
		_worldPacket.WritePackedGuid128(InviterGUID);
		_worldPacket.WritePackedGuid128(InviterBNetAccountId);
		_worldPacket.WriteUInt16(Unk1);
		_worldPacket.WriteUInt32(ProposedRoles);
		_worldPacket.WriteInt32(LfgSlots.Count);
		_worldPacket.WriteInt32(LfgCompletedMask);
		_worldPacket.WriteString(InviterName);
		foreach (int lfgSlot in LfgSlots)
		{
			_worldPacket.WriteInt32(lfgSlot);
		}
	}
}
