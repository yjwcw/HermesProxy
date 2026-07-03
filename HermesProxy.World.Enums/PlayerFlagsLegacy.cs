using System;

namespace HermesProxy.World.Enums;

[Flags]
internal enum PlayerFlagsLegacy
{
	None = 0,
	GroupLeader = 1,
	AFK = 2,
	DND = 4,
	GM = 8,
	Ghost = 0x10,
	Resting = 0x20,
	Unk7 = 0x40,
	FreeForAllPvP = 0x80,
	ContestedPvP = 0x100,
	PvPDesired = 0x200,
	HideHelm = 0x400,
	HideCloak = 0x800,
	PlayedLongTime = 0x1000,
	PlayedTooLong = 0x2000,
	Unk15 = 0x4000,
	Unk16 = 0x8000,
	Sanctuary = 0x10000,
	TaxiBenchmark = 0x20000,
	PVPTimer = 0x40000,
	Commentator = 0x80000,
	Unk21 = 0x100000,
	Unk22 = 0x200000,
	CommentatorCamera = 0x400000
}
