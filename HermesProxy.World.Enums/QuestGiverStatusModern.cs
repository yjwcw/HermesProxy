using System;

namespace HermesProxy.World.Enums;

[Flags]
public enum QuestGiverStatusModern : uint
{
	None = 0u,
	Unavailable = 2u,
	LowLevelAvailable = 4u,
	LowLevelRewardRep = 8u,
	LowLevelAvailableRep = 0x10u,
	Incomplete = 0x20u,
	IncompleteJourney = 0x40u,
	IncompleteCovenantCalling = 0x80u,
	RewardRep = 0x100u,
	AvailableRep = 0x200u,
	Available = 0x400u,
	Reward2 = 0x800u,
	Reward = 0x1000u,
	AvailableLegendaryQuest = 0x2000u,
	Reward2Legendary = 0x4000u,
	RewardLegendary = 0x8000u,
	AvailableJourney = 0x10000u,
	Reward2Journey = 0x20000u,
	RewardJourney = 0x40000u,
	AvailableCovenantCalling = 0x80000u,
	Reward2CovenantCalling = 0x100000u,
	RewardCovenantCalling = 0x200000u
}
