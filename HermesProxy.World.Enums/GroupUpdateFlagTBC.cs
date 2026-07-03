using System;

namespace HermesProxy.World.Enums;

[Flags]
public enum GroupUpdateFlagTBC : uint
{
	None = 0u,
	Status = 1u,
	CurrentHealth = 2u,
	MaxHealth = 4u,
	PowerType = 8u,
	CurrentPower = 0x10u,
	MaxPower = 0x20u,
	Level = 0x40u,
	Zone = 0x80u,
	Position = 0x100u,
	Auras = 0x200u,
	PetGuid = 0x400u,
	PetName = 0x800u,
	PetModelId = 0x1000u,
	PetCurrentHealth = 0x2000u,
	PetMaxHealth = 0x4000u,
	PetPowerType = 0x8000u,
	PetCurrentPower = 0x10000u,
	PetMaxPower = 0x20000u,
	PetAuras = 0x40000u,
	Pet = 0x7FC00u,
	Full = 0x7FFFFu
}
