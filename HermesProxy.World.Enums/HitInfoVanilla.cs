using System;

namespace HermesProxy.World.Enums;

[Flags]
public enum HitInfoVanilla : uint
{
	None = 0u,
	Unk0 = 1u,
	AffectsVictim = 2u,
	OffHand = 4u,
	Unk3 = 8u,
	Miss = 0x10u,
	FullAbsorb = 0x20u,
	FullResist = 0x40u,
	CriticalHit = 0x80u,
	Block = 0x800u,
	Glancing = 0x4000u,
	Crushing = 0x8000u,
	NoAnimation = 0x10000u,
	NoHitSound = 0x80000u
}
