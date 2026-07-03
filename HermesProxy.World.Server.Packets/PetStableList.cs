using System;
using System.Collections.Generic;
using Framework.Constants;
using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

internal class PetStableList : ServerPacket
{
	public WowGuid128 StableMaster;

	public byte NumStableSlots;

	public List<PetStableInfo> Pets = new List<PetStableInfo>();

	public PetStableList()
		: base(Opcode.SMSG_PET_STABLE_LIST, ConnectionType.Instance)
	{
	}

	public override void Write()
	{
		_worldPacket.WritePackedGuid128(StableMaster);
		_worldPacket.WriteInt32(Pets.Count);
		_worldPacket.WriteUInt8(NumStableSlots);
		foreach (PetStableInfo pet in Pets)
		{
			_worldPacket.WriteUInt32(pet.PetNumber);
			_worldPacket.WriteUInt32(pet.CreatureID);
			_worldPacket.WriteUInt32(pet.DisplayID);
			_worldPacket.WriteUInt32(pet.ExperienceLevel);
			_worldPacket.WriteUInt8(pet.LoyaltyLevel);
			_worldPacket.WriteUInt8(pet.PetFlags);
			_worldPacket.WriteBits(pet.PetName.GetByteCount(), 8);
			_worldPacket.WriteString(pet.PetName);
		}
	}
}
