using System;

namespace HermesProxy.World.Enums;

[Flags]
public enum UnitFlags2 : uint
{
	FeignDeath = 1u,
	HideBody = 2u,
	IgnoreReputation = 4u,
	ComprehendLang = 8u,
	MirrorImage = 0x10u,
	DontFadeIn = 0x20u,
	ForceMovement = 0x40u,
	DisarmOffhand = 0x80u,
	DisablePredStats = 0x100u,
	AllowChangingTalents = 0x200u,
	DisarmRanged = 0x400u,
	RegeneratePower = 0x800u,
	RestrictPartyInteraction = 0x1000u,
	PreventSpellClick = 0x2000u,
	InteractWhileHostile = 0x4000u,
	CannotTurn = 0x8000u,
	Unk2 = 0x10000u,
	PlayDeathAnim = 0x20000u,
	AllowCheatSpells = 0x40000u,
	SuppressHighlight = 0x80000u,
	TreatAsRaidUnit = 0x100000u,
	LargeAOI = 0x200000u,
	GiganticAOI = 0x400000u,
	NoActions = 0x800000u,
	AIOnlySwimIfTargetSwim = 0x1000000u,
	NoCombatLogWithNPCs = 0x2000000u,
	UntargetableByClient = 0x4000000u,
	AttackerIgnoresMinimumRanges = 0x8000000u,
	UninteractibleIfHostile = 0x10000000u,
	Unused11 = 0x20000000u,
	InfiniteAOI = 0x40000000u,
	Unused13 = 0x80000000u
}
