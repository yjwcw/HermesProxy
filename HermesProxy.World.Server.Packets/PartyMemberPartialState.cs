using System.Collections.Generic;
using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

internal class PartyMemberPartialState : ServerPacket
{
	public class PartyTypeChange
	{
		public byte PartyType1;

		public byte PartyType2;
	}

	public class Vector3_UInt16
	{
		public short X;

		public short Y;

		public short Z;
	}

	public class UnkStruct901_2
	{
		public uint Unk902_3;

		public uint Unk902_4;

		public uint Unk902_5;

		public void Write(WorldPacket data)
		{
			data.WriteUInt32(Unk902_3);
			data.WriteUInt32(Unk902_4);
			data.WriteUInt32(Unk902_5);
		}
	}

	public WowGuid128 AffectedGUID;

	public bool ForEnemyChanged;

	public bool SetPvPInactive;

	public bool Unk901_1;

	public PartyTypeChange PartyType;

	public ushort? StatusFlags;

	public byte? PowerType;

	public ushort? OverrideDisplayPower;

	public uint? CurrentHealth;

	public uint? MaxHealth;

	public ushort? CurrentPower;

	public ushort? MaxPower;

	public ushort? Level;

	public ushort? Spec;

	public ushort? ZoneID;

	public ushort? WmoGroupID;

	public uint? WmoDoodadPlacementID;

	public Vector3_UInt16 Position;

	public uint? VehicleSeatRecID;

	public List<PartyMemberAuraStates> Auras;

	public PartyMemberPetStats Pet;

	public PartyMemberPhaseStates Phase;

	public UnkStruct901_2 Unk901_2;

	public PartyMemberPartialState()
		: base(Opcode.SMSG_PARTY_MEMBER_PARTIAL_STATE)
	{
	}

	public override void Write()
	{
		_worldPacket.WriteBit(ForEnemyChanged);
		_worldPacket.WriteBit(SetPvPInactive);
		_worldPacket.WriteBit(Unk901_1);
		_worldPacket.WriteBit(PartyType != null);
		_worldPacket.WriteBit(StatusFlags.HasValue);
		_worldPacket.WriteBit(PowerType.HasValue);
		_worldPacket.WriteBit(OverrideDisplayPower.HasValue);
		_worldPacket.WriteBit(CurrentHealth.HasValue);
		_worldPacket.WriteBit(MaxHealth.HasValue);
		_worldPacket.WriteBit(CurrentPower.HasValue);
		_worldPacket.WriteBit(MaxPower.HasValue);
		_worldPacket.WriteBit(Level.HasValue);
		_worldPacket.WriteBit(Spec.HasValue);
		_worldPacket.WriteBit(ZoneID.HasValue);
		_worldPacket.WriteBit(WmoGroupID.HasValue);
		_worldPacket.WriteBit(WmoDoodadPlacementID.HasValue);
		_worldPacket.WriteBit(Position != null);
		_worldPacket.WriteBit(VehicleSeatRecID.HasValue);
		_worldPacket.WriteBit(Auras != null);
		_worldPacket.WriteBit(Pet != null);
		_worldPacket.WriteBit(Phase != null);
		_worldPacket.WriteBit(Unk901_2 != null);
		_worldPacket.FlushBits();
		if (Pet != null)
		{
			Pet.WritePartial(_worldPacket);
		}
		_worldPacket.WritePackedGuid128(AffectedGUID);
		if (PartyType != null)
		{
			_worldPacket.WriteUInt8(PartyType.PartyType1);
			_worldPacket.WriteUInt8(PartyType.PartyType2);
		}
		if (StatusFlags.HasValue)
		{
			_worldPacket.WriteUInt16(StatusFlags.Value);
		}
		if (PowerType.HasValue)
		{
			_worldPacket.WriteUInt8(PowerType.Value);
		}
		if (OverrideDisplayPower.HasValue)
		{
			_worldPacket.WriteUInt16(OverrideDisplayPower.Value);
		}
		if (CurrentHealth.HasValue)
		{
			_worldPacket.WriteUInt32(CurrentHealth.Value);
		}
		if (MaxHealth.HasValue)
		{
			_worldPacket.WriteUInt32(MaxHealth.Value);
		}
		if (CurrentPower.HasValue)
		{
			_worldPacket.WriteUInt16(CurrentPower.Value);
		}
		if (MaxPower.HasValue)
		{
			_worldPacket.WriteUInt16(MaxPower.Value);
		}
		if (Level.HasValue)
		{
			_worldPacket.WriteUInt16(Level.Value);
		}
		if (Spec.HasValue)
		{
			_worldPacket.WriteUInt16(Spec.Value);
		}
		if (ZoneID.HasValue)
		{
			_worldPacket.WriteUInt16(ZoneID.Value);
		}
		if (WmoGroupID.HasValue)
		{
			_worldPacket.WriteUInt16(WmoGroupID.Value);
		}
		if (WmoDoodadPlacementID.HasValue)
		{
			_worldPacket.WriteUInt32(WmoDoodadPlacementID.Value);
		}
		if (Position != null)
		{
			_worldPacket.WriteInt16(Position.X);
			_worldPacket.WriteInt16(Position.Y);
			_worldPacket.WriteInt16(Position.Z);
		}
		if (VehicleSeatRecID.HasValue)
		{
			_worldPacket.WriteUInt32(VehicleSeatRecID.Value);
		}
		if (Auras != null)
		{
			_worldPacket.WriteInt32(Auras.Count);
			foreach (PartyMemberAuraStates aura in Auras)
			{
				aura.Write(_worldPacket);
			}
		}
		if (Phase != null)
		{
			Phase.Write(_worldPacket);
		}
		if (Unk901_2 != null)
		{
			Unk901_2.Write(_worldPacket);
		}
	}
}
