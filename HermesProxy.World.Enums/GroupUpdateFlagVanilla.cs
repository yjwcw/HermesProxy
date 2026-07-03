using System;

namespace HermesProxy.World.Enums;

[Flags]
public enum GroupUpdateFlagVanilla : uint
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
	AurasNegative = 0x400u,
	PetGuid = 0x800u,
	PetName = 0x1000u,
	PetModelId = 0x2000u,
	PetCurrentHealth = 0x4000u,
	PetMaxHealth = 0x8000u,
	PetPowerType = 0x10000u,
	PetCurrentPower = 0x20000u,
	PetMaxPower = 0x40000u,
	PetAuras = 0x80000u,
	PetAurasNegative = 0x100000u
}
