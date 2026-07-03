using System;

namespace HermesProxy.World.Enums;

[Flags]
public enum HitInfo : uint
{
	None = 0u,
	Unk0 = 1u,
	AffectsVictim = 2u,
	OffHand = 4u,
	Unk3 = 8u,
	Miss = 0x10u,
	FullAbsorb = 0x20u,
	PartialAbsorb = 0x40u,
	FullResist = 0x80u,
	PartialResist = 0x100u,
	CriticalHit = 0x200u,
	Unk10 = 0x400u,
	Unk11 = 0x800u,
	Unk12 = 0x1000u,
	Block = 0x2000u,
	Unk14 = 0x4000u,
	Unk15 = 0x8000u,
	Glancing = 0x10000u,
	Crushing = 0x20000u,
	NoAnimation = 0x40000u,
	Unk19 = 0x80000u,
	Unk20 = 0x100000u,
	NoHitSound = 0x200000u,
	Unk22 = 0x400000u,
	RageGain = 0x800000u,
	FakeDamage = 0x1000000u,
	Unk25 = 0x2000000u,
	Unk26 = 0x4000000u
}
