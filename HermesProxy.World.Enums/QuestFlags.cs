using System;

namespace HermesProxy.World.Enums;

[Flags]
public enum QuestFlags : uint
{
	None = 0u,
	StayAlive = 1u,
	PartyAccept = 2u,
	Exploration = 4u,
	Sharable = 8u,
	HasCondition = 0x10u,
	HideRewardPOI = 0x20u,
	Raid = 0x40u,
	ExpansionOnly = 0x80u,
	NoMoneyFromExperience = 0x100u,
	HiddenRewards = 0x200u,
	Tracking = 0x400u,
	DepricateReputation = 0x800u,
	Daily = 0x1000u,
	FlagsForPvP = 0x2000u,
	Unavailable = 0x4000u,
	Weekly = 0x8000u,
	AutoComplete = 0x10000u,
	DisplayItemInTracker = 0x20000u,
	DisableCompletionText = 0x40000u,
	AutoAccept = 0x80000u,
	PlayerCastOnAccept = 0x100000u,
	PlayerCastOnComplete = 0x200000u,
	UpdatePhaseShift = 0x400000u,
	SoRWhitelist = 0x800000u,
	LaunchGossipComplete = 0x1000000u,
	RemoveExtraGetItems = 0x2000000u,
	HideUntilDiscovered = 0x4000000u,
	PortraitInQuestLog = 0x8000000u,
	ShowItemWhenCompleted = 0x10000000u,
	LaunchGossipAccept = 0x20000000u,
	ItemsGlowWhenDone = 0x40000000u,
	FailOnLogout = 0x80000000u
}
