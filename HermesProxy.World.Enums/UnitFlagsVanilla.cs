using System;

namespace HermesProxy.World.Enums;

[Flags]
public enum UnitFlagsVanilla : uint
{
	None = 0u,
	ServerControlled = 1u,
	Spawning = 2u,
	RemoveClientControl = 4u,
	PlayerControlled = 8u,
	PetRename = 0x10u,
	PetAbandon = 0x20u,
	Unk6 = 0x40u,
	NotAttackable1 = 0x80u,
	ImmuneToPc = 0x100u,
	ImmuneToNpc = 0x200u,
	Looting = 0x400u,
	PetInCombat = 0x800u,
	Pvp = 0x1000u,
	Silenced = 0x2000u,
	CannotSwim = 0x4000u,
	CanSwim = 0x8000u,
	NotAttackable2 = 0x10000u,
	Pacified = 0x20000u,
	Stunned = 0x40000u,
	InCombat = 0x80000u,
	TaxiFlight = 0x100000u,
	Disarmed = 0x200000u,
	Confused = 0x400000u,
	Fleeing = 0x800000u,
	Possessed = 0x1000000u,
	NotSelectable = 0x2000000u,
	Skinnable = 0x4000000u,
	AurasVisible = 0x8000000u,
	Unk28 = 0x10000000u,
	PreventAnim = 0x20000000u,
	Sheathe = 0x40000000u,
	Immune = 0x80000000u
}
