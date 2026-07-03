using System;
using System.Collections.Generic;

namespace HermesProxy.World.Server.Packets;

public class PartyMemberPetStats
{
	public WowGuid128 NewPetGuid;

	public string NewPetName;

	public uint? DisplayID;

	public uint? MaxHealth;

	public uint? Health;

	public List<PartyMemberAuraStates> Auras;

	public void WritePartial(WorldPacket data)
	{
		data.WriteBit(NewPetGuid != null);
		data.WriteBit(NewPetName != null);
		data.WriteBit(DisplayID.HasValue);
		data.WriteBit(MaxHealth.HasValue);
		data.WriteBit(Health.HasValue);
		data.WriteBit(Auras != null);
		data.FlushBits();
		if (NewPetName != null)
		{
			data.WriteBits(NewPetName.GetByteCount(), 8);
			data.WriteString(NewPetName);
		}
		if (NewPetGuid != null)
		{
			data.WritePackedGuid128(NewPetGuid);
		}
		if (DisplayID.HasValue)
		{
			data.WriteUInt32(DisplayID.Value);
		}
		if (MaxHealth.HasValue)
		{
			data.WriteUInt32(MaxHealth.Value);
		}
		if (Health.HasValue)
		{
			data.WriteUInt32(Health.Value);
		}
		if (Auras == null)
		{
			return;
		}
		data.WriteInt32(Auras.Count);
		foreach (PartyMemberAuraStates aura in Auras)
		{
			aura.Write(data);
		}
	}

	public void WriteFull(WorldPacket data)
	{
		if (NewPetGuid == null)
		{
			NewPetGuid = WowGuid128.Empty;
		}
		if (NewPetName == null)
		{
			NewPetName = "";
		}
		if (!DisplayID.HasValue)
		{
			DisplayID = 0u;
		}
		if (!MaxHealth.HasValue)
		{
			MaxHealth = 0u;
		}
		if (!Health.HasValue)
		{
			Health = 0u;
		}
		if (Auras == null)
		{
			Auras = new List<PartyMemberAuraStates>();
		}
		data.WritePackedGuid128(NewPetGuid);
		data.WriteUInt32(DisplayID.Value);
		data.WriteUInt32(Health.Value);
		data.WriteUInt32(MaxHealth.Value);
		data.WriteInt32(Auras.Count);
		Auras.ForEach(delegate(PartyMemberAuraStates p)
		{
			p.Write(data);
		});
		data.WriteBits(NewPetName.GetByteCount(), 8);
		data.FlushBits();
		data.WriteString(NewPetName);
	}
}
