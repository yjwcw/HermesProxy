using System.Collections.Generic;
using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

internal class PartyMemberFullState : ServerPacket
{
	public bool ForEnemy;

	public ushort Level;

	public GroupMemberOnlineStatus StatusFlags;

	public int CurrentHealth;

	public int MaxHealth;

	public byte PowerType;

	public ushort CurrentPower;

	public ushort MaxPower;

	public ushort ZoneID;

	public short PositionX;

	public short PositionY;

	public short PositionZ;

	public int VehicleSeat;

	public PartyMemberPhaseStates Phases = new PartyMemberPhaseStates();

	public List<PartyMemberAuraStates> Auras = new List<PartyMemberAuraStates>();

	public PartyMemberPetStats Pet;

	public ushort PowerDisplayID;

	public ushort SpecID;

	public ushort WmoGroupID;

	public int WmoDoodadPlacementID;

	public sbyte[] PartyType = new sbyte[2];

	public CTROptions ChromieTime;

	public WowGuid128 MemberGuid;

	public PartyMemberFullState()
		: base(Opcode.SMSG_PARTY_MEMBER_FULL_STATE)
	{
		Phases.PhaseShiftFlags = 8u;
	}

	public override void Write()
	{
		_worldPacket.WriteBit(ForEnemy);
		_worldPacket.FlushBits();
		for (byte b = 0; b < 2; b++)
		{
			_worldPacket.WriteInt8(PartyType[b]);
		}
		_worldPacket.WriteInt16((short)StatusFlags);
		_worldPacket.WriteUInt8(PowerType);
		_worldPacket.WriteInt16((short)PowerDisplayID);
		_worldPacket.WriteInt32(CurrentHealth);
		_worldPacket.WriteInt32(MaxHealth);
		_worldPacket.WriteUInt16(CurrentPower);
		_worldPacket.WriteUInt16(MaxPower);
		_worldPacket.WriteUInt16(Level);
		_worldPacket.WriteUInt16(SpecID);
		_worldPacket.WriteUInt16(ZoneID);
		_worldPacket.WriteUInt16(WmoGroupID);
		_worldPacket.WriteInt32(WmoDoodadPlacementID);
		_worldPacket.WriteInt16(PositionX);
		_worldPacket.WriteInt16(PositionY);
		_worldPacket.WriteInt16(PositionZ);
		_worldPacket.WriteInt32(VehicleSeat);
		_worldPacket.WriteInt32(Auras.Count);
		Phases.Write(_worldPacket);
		ChromieTime.Write(_worldPacket);
		foreach (PartyMemberAuraStates aura in Auras)
		{
			aura.Write(_worldPacket);
		}
		_worldPacket.WriteBit(Pet != null);
		_worldPacket.FlushBits();
		if (Pet != null)
		{
			Pet.WriteFull(_worldPacket);
		}
		_worldPacket.WritePackedGuid128(MemberGuid);
	}
}
