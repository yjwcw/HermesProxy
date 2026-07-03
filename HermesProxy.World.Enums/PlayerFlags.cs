using System;

namespace HermesProxy.World.Enums;

[Flags]
public enum PlayerFlags : uint
{
	None = 0u,
	GroupLeader = 1u,
	AFK = 2u,
	DND = 4u,
	GM = 8u,
	Ghost = 0x10u,
	Resting = 0x20u,
	VoiceChat = 0x40u,
	Unk7 = 0x80u,
	ContestedPvP = 0x100u,
	PvPDesired = 0x200u,
	WarModeActive = 0x400u,
	WarModeDesired = 0x800u,
	PlayedLongTime = 0x1000u,
	PlayedTooLong = 0x2000u,
	IsOutOfBounds = 0x4000u,
	Developer = 0x8000u,
	LowLevelRaidEnabled = 0x10000u,
	TaxiBenchmark = 0x20000u,
	PVPTimer = 0x40000u,
	Uber = 0x80000u,
	Unk20 = 0x100000u,
	Unk21 = 0x200000u,
	Commentator = 0x400000u,
	HideAccountAchievements = 0x800000u,
	PetBattlesUnlocked = 0x1000000u,
	NoXPGain = 0x2000000u,
	Unk26 = 0x4000000u,
	AutoDeclineGuild = 0x8000000u,
	GuildLevelEnabled = 0x10000000u,
	VoidUnlocked = 0x20000000u,
	Timewalking = 0x40000000u,
	CommentatorCamera = 0x80000000u
}
