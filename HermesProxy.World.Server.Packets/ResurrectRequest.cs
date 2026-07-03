using System;
using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

internal class ResurrectRequest : ServerPacket
{
	public WowGuid128 CasterGUID;

	public uint CasterVirtualRealmAddress;

	public uint PetNumber;

	public uint SpellID;

	public bool UseTimer;

	public bool Sickness;

	public string Name;

	public ResurrectRequest()
		: base(Opcode.SMSG_RESURRECT_REQUEST)
	{
	}

	public override void Write()
	{
		_worldPacket.WritePackedGuid128(CasterGUID);
		_worldPacket.WriteUInt32(CasterVirtualRealmAddress);
		_worldPacket.WriteUInt32(PetNumber);
		_worldPacket.WriteUInt32(SpellID);
		_worldPacket.WriteBits(Name.GetByteCount(), 11);
		_worldPacket.WriteBit(UseTimer);
		_worldPacket.WriteBit(Sickness);
		_worldPacket.FlushBits();
		_worldPacket.WriteString(Name);
	}
}
